using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/authorization-methods")]
public class AuthorizationMethodsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AuthorizationMethodsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tenantId = HttpContext.Items["TenantId"] as Guid?;
        if (tenantId == null)
            return BadRequest("Tenant not found");

        var methods = await _context.AuthorizationMethods
            .Include(am => am.User)
            .Where(am => am.User.TenantId == tenantId.Value)
            .Select(am => new
            {
                am.Id,
                am.UserId,
                UserName = $"{am.User.FirstName} {am.User.LastName}",
                am.User.Email,
                Type = am.Type.ToString(),
                am.Identifier,
                am.FriendlyName,
                am.IsActive,
                am.ValidFrom,
                am.ValidUntil,
                am.CreatedAt,
                am.LastUsedAt
            })
            .ToListAsync();

        return Ok(methods);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        var methods = await _context.AuthorizationMethods
            .Where(am => am.UserId == userId && am.IsActive)
            .Select(am => new
            {
                am.Id,
                Type = am.Type.ToString(),
                am.Identifier,
                am.FriendlyName,
                am.IsActive,
                am.ValidFrom,
                am.ValidUntil,
                am.CreatedAt,
                am.LastUsedAt
            })
            .ToListAsync();

        return Ok(methods);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAuthorizationMethodDto dto)
    {
        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
            return NotFound("User not found");

        // Check for duplicate identifier
        var exists = await _context.AuthorizationMethods
            .AnyAsync(am => am.Identifier == dto.Identifier && am.Type == dto.Type);

        if (exists)
            return BadRequest("An authorization method with this identifier already exists");

        var method = new AuthorizationMethod
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Type = dto.Type,
            Identifier = dto.Identifier,
            FriendlyName = dto.FriendlyName,
            IsActive = true,
            ValidFrom = dto.ValidFrom,
            ValidUntil = dto.ValidUntil,
            Metadata = dto.Metadata,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuthorizationMethods.Add(method);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByUser), new { userId = method.UserId }, new
        {
            method.Id,
            method.UserId,
            Type = method.Type.ToString(),
            method.Identifier,
            method.FriendlyName,
            method.IsActive,
            method.ValidFrom,
            method.ValidUntil,
            method.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAuthorizationMethodDto dto)
    {
        var method = await _context.AuthorizationMethods.FindAsync(id);
        if (method == null)
            return NotFound();

        method.FriendlyName = dto.FriendlyName;
        method.IsActive = dto.IsActive;
        method.ValidFrom = dto.ValidFrom;
        method.ValidUntil = dto.ValidUntil;
        method.Metadata = dto.Metadata;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            method.Id,
            method.UserId,
            Type = method.Type.ToString(),
            method.Identifier,
            method.FriendlyName,
            method.IsActive,
            method.ValidFrom,
            method.ValidUntil,
            method.CreatedAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var method = await _context.AuthorizationMethods.FindAsync(id);
        if (method == null)
            return NotFound();

        method.IsActive = false; // Soft delete
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyAuthorizationDto dto)
    {
        var method = await _context.AuthorizationMethods
            .Include(am => am.User)
            .FirstOrDefaultAsync(am => 
                am.Identifier == dto.Identifier && 
                am.Type == dto.Type &&
                am.IsActive &&
                (am.ValidFrom == null || am.ValidFrom <= DateTime.UtcNow) &&
                (am.ValidUntil == null || am.ValidUntil >= DateTime.UtcNow));

        if (method == null)
            return NotFound("Authorization method not found or expired");

        // Update last used
        method.LastUsedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            method.Id,
            method.UserId,
            UserName = $"{method.User.FirstName} {method.User.LastName}",
            method.User.Email,
            Type = method.Type.ToString(),
            Authorized = true
        });
    }
}

public record CreateAuthorizationMethodDto(
    Guid UserId,
    AuthorizationMethodType Type,
    string Identifier,
    string? FriendlyName,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    string? Metadata
);

public record UpdateAuthorizationMethodDto(
    string? FriendlyName,
    bool IsActive,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    string? Metadata
);

public record VerifyAuthorizationDto(
    AuthorizationMethodType Type,
    string Identifier
);

