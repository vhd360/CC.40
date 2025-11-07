using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChargingControlSystem.Api.Services;
using ChargingControlSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<UploadController> _logger;

    public UploadController(
        ApplicationDbContext context,
        IWebHostEnvironment env,
        ILogger<UploadController> logger)
    {
        _context = context;
        _env = env;
        _logger = logger;
    }

    [HttpPost("tenant-logo")]
    public async Task<IActionResult> UploadTenantLogo(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Keine Datei hochgeladen" });
            }

            // Validierung: nur Bilder erlaubt
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Nur Bilddateien (jpg, png, svg, webp) sind erlaubt" });
            }

            // Max. 5 MB
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { message = "Datei zu groß. Maximum: 5 MB" });
            }

            // Tenant-ID aus Claims holen
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                return Unauthorized(new { message = "Tenant nicht gefunden" });
            }

            // Tenant aus DB laden
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
            {
                return NotFound(new { message = "Tenant nicht gefunden" });
            }

            // Upload-Verzeichnis erstellen
            var uploadsPath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads", "tenants", tenantId.ToString());
            Directory.CreateDirectory(uploadsPath);

            // Altes Logo löschen, falls vorhanden
            if (!string.IsNullOrEmpty(tenant.LogoUrl))
            {
                var oldLogoPath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, tenant.LogoUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldLogoPath))
                {
                    try
                    {
                        System.IO.File.Delete(oldLogoPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Fehler beim Löschen des alten Logos");
                    }
                }
            }

            // Neuen Dateinamen generieren
            var fileName = $"logo_{DateTime.UtcNow.Ticks}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Datei speichern
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // URL für die Datenbank
            var logoUrl = $"/uploads/tenants/{tenantId}/{fileName}";
            
            // Tenant aktualisieren
            tenant.LogoUrl = logoUrl;
            tenant.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { 
                logoUrl = logoUrl,
                message = "Logo erfolgreich hochgeladen" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Hochladen des Logos");
            return StatusCode(500, new { message = "Fehler beim Hochladen des Logos" });
        }
    }

    [HttpDelete("tenant-logo")]
    public async Task<IActionResult> DeleteTenantLogo()
    {
        try
        {
            // Tenant-ID aus Claims holen
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                return Unauthorized(new { message = "Tenant nicht gefunden" });
            }

            // Tenant aus DB laden
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
            {
                return NotFound(new { message = "Tenant nicht gefunden" });
            }

            // Logo löschen, falls vorhanden
            if (!string.IsNullOrEmpty(tenant.LogoUrl))
            {
                var logoPath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, tenant.LogoUrl.TrimStart('/'));
                if (System.IO.File.Exists(logoPath))
                {
                    try
                    {
                        System.IO.File.Delete(logoPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Fehler beim Löschen des Logos");
                    }
                }
            }

            // Tenant aktualisieren
            tenant.LogoUrl = null;
            tenant.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Logo erfolgreich gelöscht" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen des Logos");
            return StatusCode(500, new { message = "Fehler beim Löschen des Logos" });
        }
    }
}


