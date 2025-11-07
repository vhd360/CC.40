using ChargingControlSystem.Api.Services;
using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TariffsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly ILogger<TariffsController> _logger;

    public TariffsController(
        ApplicationDbContext context,
        ITenantService tenantService,
        ILogger<TariffsController> logger)
    {
        _context = context;
        _tenantService = tenantService;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Liste aller Tarife", Description = "Gibt alle Tarife des aktuellen Tenants zurück")]
    [SwaggerResponse(200, "Liste der Tarife", typeof(IEnumerable<TariffDto>))]
    public async Task<ActionResult<IEnumerable<TariffDto>>> GetTariffs()
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var tariffs = await _context.Tariffs
            .Include(t => t.Components)
            .Include(t => t.UserGroupTariffs)
                .ThenInclude(ugt => ugt.UserGroup)
            .Where(t => t.TenantId == tenantId)
            .OrderByDescending(t => t.IsDefault)
            .ThenBy(t => t.Name)
            .ToListAsync();

        return Ok(tariffs.Select(t => new TariffDto(t)));
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Tarif abrufen", Description = "Gibt einen einzelnen Tarif mit Details zurück")]
    [SwaggerResponse(200, "Tarif gefunden", typeof(TariffDto))]
    [SwaggerResponse(404, "Tarif nicht gefunden")]
    public async Task<ActionResult<TariffDto>> GetTariff(Guid id)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var tariff = await _context.Tariffs
            .Include(t => t.Components)
            .Include(t => t.UserGroupTariffs)
                .ThenInclude(ugt => ugt.UserGroup)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);

        if (tariff == null)
            return NotFound();

        return Ok(new TariffDto(tariff));
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Tarif erstellen", Description = "Erstellt einen neuen Tarif")]
    [SwaggerResponse(201, "Tarif erstellt", typeof(TariffDto))]
    [SwaggerResponse(400, "Ungültige Daten")]
    public async Task<ActionResult<TariffDto>> CreateTariff(CreateTariffRequest request)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        // Check if name already exists
        if (await _context.Tariffs.AnyAsync(t => t.TenantId == tenantId && t.Name == request.Name))
            return BadRequest("Ein Tarif mit diesem Namen existiert bereits");

        var tariff = new Tariff
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            Currency = request.Currency ?? "EUR",
            IsDefault = request.IsDefault,
            IsActive = request.IsActive,
            ValidFrom = request.ValidFrom,
            ValidUntil = request.ValidUntil,
            CreatedAt = DateTime.UtcNow
        };

        // Add components
        foreach (var componentRequest in request.Components)
        {
            tariff.Components.Add(new TariffComponent
            {
                Id = Guid.NewGuid(),
                Type = componentRequest.Type,
                Price = componentRequest.Price,
                StepSize = componentRequest.StepSize,
                TimeStart = componentRequest.TimeStart,
                TimeEnd = componentRequest.TimeEnd,
                DaysOfWeek = componentRequest.DaysOfWeek,
                MinimumCharge = componentRequest.MinimumCharge,
                MaximumCharge = componentRequest.MaximumCharge,
                GracePeriodMinutes = componentRequest.GracePeriodMinutes,
                DisplayOrder = componentRequest.DisplayOrder,
                IsActive = true
            });
        }

        _context.Tariffs.Add(tariff);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created tariff {TariffId} ({TariffName})", tariff.Id, tariff.Name);

        return CreatedAtAction(nameof(GetTariff), new { id = tariff.Id }, new TariffDto(tariff));
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Tarif aktualisieren", Description = "Aktualisiert einen bestehenden Tarif")]
    [SwaggerResponse(200, "Tarif aktualisiert", typeof(TariffDto))]
    [SwaggerResponse(404, "Tarif nicht gefunden")]
    public async Task<ActionResult<TariffDto>> UpdateTariff(Guid id, UpdateTariffRequest request)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var tariff = await _context.Tariffs
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);

        if (tariff == null)
            return NotFound();

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Update tariff properties
            tariff.Name = request.Name;
            tariff.Description = request.Description;
            tariff.Currency = request.Currency ?? "EUR";
            tariff.IsDefault = request.IsDefault;
            tariff.IsActive = request.IsActive;
            tariff.ValidFrom = request.ValidFrom;
            tariff.ValidUntil = request.ValidUntil;
            tariff.UpdatedAt = DateTime.UtcNow;

            // Delete existing components directly via SQL to avoid tracking issues
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM TariffComponents WHERE TariffId = {0}", id);

            // Add new components
            foreach (var componentRequest in request.Components)
            {
                var newComponent = new TariffComponent
                {
                    Id = Guid.NewGuid(),
                    TariffId = id,
                    Type = componentRequest.Type,
                    Price = componentRequest.Price,
                    StepSize = componentRequest.StepSize,
                    TimeStart = componentRequest.TimeStart,
                    TimeEnd = componentRequest.TimeEnd,
                    DaysOfWeek = componentRequest.DaysOfWeek,
                    MinimumCharge = componentRequest.MinimumCharge,
                    MaximumCharge = componentRequest.MaximumCharge,
                    GracePeriodMinutes = componentRequest.GracePeriodMinutes,
                    DisplayOrder = componentRequest.DisplayOrder,
                    IsActive = true
                };
                _context.TariffComponents.Add(newComponent);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Updated tariff {TariffId} ({TariffName})", tariff.Id, tariff.Name);

            // Reload tariff with all data
            var updatedTariff = await _context.Tariffs
                .Include(t => t.Components)
                .Include(t => t.UserGroupTariffs)
                    .ThenInclude(ugt => ugt.UserGroup)
                .FirstOrDefaultAsync(t => t.Id == id);

            return Ok(new TariffDto(updatedTariff!));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating tariff {TariffId}", id);
            throw;
        }
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Tarif löschen", Description = "Löscht einen Tarif")]
    [SwaggerResponse(204, "Tarif gelöscht")]
    [SwaggerResponse(404, "Tarif nicht gefunden")]
    public async Task<IActionResult> DeleteTariff(Guid id)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var tariff = await _context.Tariffs
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);

        if (tariff == null)
            return NotFound();

        _context.Tariffs.Remove(tariff);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted tariff {TariffId} ({TariffName})", tariff.Id, tariff.Name);

        return NoContent();
    }

    [HttpPost("{tariffId}/usergroups/{userGroupId}")]
    [SwaggerOperation(Summary = "Tarif einer Benutzergruppe zuweisen", Description = "Weist einen Tarif einer Benutzergruppe zu")]
    [SwaggerResponse(200, "Tarif zugewiesen")]
    [SwaggerResponse(404, "Tarif oder Benutzergruppe nicht gefunden")]
    [SwaggerResponse(409, "Zuordnung existiert bereits")]
    public async Task<IActionResult> AssignTariffToUserGroup(Guid tariffId, Guid userGroupId, [FromBody] int priority = 0)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var tariff = await _context.Tariffs
            .FirstOrDefaultAsync(t => t.Id == tariffId && t.TenantId == tenantId);

        if (tariff == null)
            return NotFound("Tarif nicht gefunden");

        var userGroup = await _context.UserGroups
            .FirstOrDefaultAsync(ug => ug.Id == userGroupId && ug.TenantId == tenantId);

        if (userGroup == null)
            return NotFound("Benutzergruppe nicht gefunden");

        // Check if assignment already exists
        if (await _context.UserGroupTariffs.AnyAsync(ugt => ugt.TariffId == tariffId && ugt.UserGroupId == userGroupId))
            return Conflict("Tarif ist dieser Benutzergruppe bereits zugewiesen");

        var assignment = new UserGroupTariff
        {
            Id = Guid.NewGuid(),
            TariffId = tariffId,
            UserGroupId = userGroupId,
            Priority = priority,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserGroupTariffs.Add(assignment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Assigned tariff {TariffId} to user group {UserGroupId} with priority {Priority}",
            tariffId, userGroupId, priority);

        return Ok();
    }

    [HttpDelete("{tariffId}/usergroups/{userGroupId}")]
    [SwaggerOperation(Summary = "Tarifzuweisung entfernen", Description = "Entfernt die Zuweisung eines Tarifs von einer Benutzergruppe")]
    [SwaggerResponse(204, "Zuweisung entfernt")]
    [SwaggerResponse(404, "Zuweisung nicht gefunden")]
    public async Task<IActionResult> RemoveTariffFromUserGroup(Guid tariffId, Guid userGroupId)
    {
        var assignment = await _context.UserGroupTariffs
            .FirstOrDefaultAsync(ugt => ugt.TariffId == tariffId && ugt.UserGroupId == userGroupId);

        if (assignment == null)
            return NotFound();

        _context.UserGroupTariffs.Remove(assignment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed tariff {TariffId} from user group {UserGroupId}", tariffId, userGroupId);

        return NoContent();
    }
}

// DTOs
public record TariffDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Currency { get; init; } = "EUR";
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidUntil { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public List<TariffComponentDto> Components { get; init; } = new();
    public List<UserGroupTariffDto> UserGroups { get; init; } = new();

    public TariffDto() { }

    public TariffDto(Tariff tariff)
    {
        Id = tariff.Id;
        Name = tariff.Name;
        Description = tariff.Description;
        Currency = tariff.Currency;
        IsDefault = tariff.IsDefault;
        IsActive = tariff.IsActive;
        ValidFrom = tariff.ValidFrom;
        ValidUntil = tariff.ValidUntil;
        CreatedAt = tariff.CreatedAt;
        UpdatedAt = tariff.UpdatedAt;
        Components = tariff.Components.Select(c => new TariffComponentDto(c)).ToList();
        UserGroups = tariff.UserGroupTariffs.Select(ugt => new UserGroupTariffDto
        {
            UserGroupId = ugt.UserGroupId,
            UserGroupName = ugt.UserGroup?.Name ?? "",
            Priority = ugt.Priority
        }).ToList();
    }
}

public record TariffComponentDto
{
    public Guid Id { get; init; }
    public TariffComponentType Type { get; init; }
    public decimal Price { get; init; }
    public int? StepSize { get; init; }
    public string? TimeStart { get; init; }
    public string? TimeEnd { get; init; }
    public string? DaysOfWeek { get; init; }
    public decimal? MinimumCharge { get; init; }
    public decimal? MaximumCharge { get; init; }
    public int? GracePeriodMinutes { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }

    public TariffComponentDto() { }

    public TariffComponentDto(TariffComponent component)
    {
        Id = component.Id;
        Type = component.Type;
        Price = component.Price;
        StepSize = component.StepSize;
        TimeStart = component.TimeStart;
        TimeEnd = component.TimeEnd;
        DaysOfWeek = component.DaysOfWeek;
        MinimumCharge = component.MinimumCharge;
        MaximumCharge = component.MaximumCharge;
        GracePeriodMinutes = component.GracePeriodMinutes;
        DisplayOrder = component.DisplayOrder;
        IsActive = component.IsActive;
    }
}

public record UserGroupTariffDto
{
    public Guid UserGroupId { get; init; }
    public string UserGroupName { get; init; } = string.Empty;
    public int Priority { get; init; }
}

public record CreateTariffRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Currency { get; init; } = "EUR";
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidUntil { get; init; }
    public List<TariffComponentRequest> Components { get; init; } = new();
}

public record UpdateTariffRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Currency { get; init; } = "EUR";
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidUntil { get; init; }
    public List<TariffComponentRequest> Components { get; init; } = new();
}

public record TariffComponentRequest
{
    public TariffComponentType Type { get; init; }
    public decimal Price { get; init; }
    public int? StepSize { get; init; }
    public string? TimeStart { get; init; }
    public string? TimeEnd { get; init; }
    public string? DaysOfWeek { get; init; }
    public decimal? MinimumCharge { get; init; }
    public decimal? MaximumCharge { get; init; }
    public int? GracePeriodMinutes { get; init; }
    public int DisplayOrder { get; init; }
}

