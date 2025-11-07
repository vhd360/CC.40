using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChargingControlSystem.Data;
using ChargingControlSystem.Data.Entities;
using ChargingControlSystem.Api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace ChargingControlSystem.Api.Controllers;

[ApiController]
[Route("api/vehicle-assignments")]
[SwaggerTag("Verwaltung von Fahrzeugzuweisungen (Dienstwagen, Poolfahrzeuge)")]
public class VehicleAssignmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly ILogger<VehicleAssignmentsController> _logger;

    public VehicleAssignmentsController(
        ApplicationDbContext context,
        ITenantService tenantService,
        ILogger<VehicleAssignmentsController> logger)
    {
        _context = context;
        _tenantService = tenantService;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Alle Fahrzeugzuweisungen abrufen",
        Description = "Gibt eine Liste aller Fahrzeugzuweisungen des aktuellen Tenants zurück."
    )]
    [SwaggerResponse(200, "Liste der Zuweisungen")]
    public async Task<IActionResult> GetAll([FromQuery] bool includeReturned = false)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var query = _context.VehicleAssignments
            .Include(va => va.Vehicle)
            .Include(va => va.User)
            .Where(va => va.Vehicle.TenantId == tenantId);

        if (!includeReturned)
        {
            query = query.Where(va => va.ReturnedAt == null);
        }

        var assignments = await query
            .OrderByDescending(va => va.AssignedAt)
            .Select(va => new
            {
                va.Id,
                va.VehicleId,
                Vehicle = new
                {
                    va.Vehicle.Id,
                    va.Vehicle.LicensePlate,
                    va.Vehicle.Make,
                    va.Vehicle.Model,
                    Type = va.Vehicle.Type.ToString()
                },
                va.UserId,
                User = new
                {
                    va.User.Id,
                    va.User.FirstName,
                    va.User.LastName,
                    va.User.Email
                },
                AssignmentType = va.AssignmentType.ToString(),
                va.AssignedAt,
                va.ReturnedAt,
                va.Notes,
                IsActive = va.ReturnedAt == null
            })
            .ToListAsync();

        return Ok(assignments);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Fahrzeugzuweisung nach ID abrufen",
        Description = "Gibt Details zu einer bestimmten Fahrzeugzuweisung zurück."
    )]
    [SwaggerResponse(200, "Zuweisungs-Details")]
    [SwaggerResponse(404, "Zuweisung nicht gefunden")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var assignment = await _context.VehicleAssignments
            .Include(va => va.Vehicle)
            .Include(va => va.User)
            .Where(va => va.Id == id && va.Vehicle.TenantId == tenantId)
            .Select(va => new
            {
                va.Id,
                va.VehicleId,
                Vehicle = new
                {
                    va.Vehicle.Id,
                    va.Vehicle.LicensePlate,
                    va.Vehicle.Make,
                    va.Vehicle.Model,
                    va.Vehicle.Year,
                    va.Vehicle.Color,
                    Type = va.Vehicle.Type.ToString()
                },
                va.UserId,
                User = new
                {
                    va.User.Id,
                    va.User.FirstName,
                    va.User.LastName,
                    va.User.Email,
                    va.User.PhoneNumber
                },
                AssignmentType = va.AssignmentType.ToString(),
                va.AssignedAt,
                va.ReturnedAt,
                va.Notes,
                IsActive = va.ReturnedAt == null
            })
            .FirstOrDefaultAsync();

        if (assignment == null)
            return NotFound("Fahrzeugzuweisung nicht gefunden");

        return Ok(assignment);
    }

    [HttpGet("vehicle/{vehicleId}")]
    [SwaggerOperation(
        Summary = "Zuweisungen für ein Fahrzeug abrufen",
        Description = "Gibt alle Zuweisungen für ein bestimmtes Fahrzeug zurück."
    )]
    [SwaggerResponse(200, "Liste der Zuweisungen")]
    public async Task<IActionResult> GetByVehicle(Guid vehicleId, [FromQuery] bool includeReturned = false)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == vehicleId && v.TenantId == tenantId);

        if (vehicle == null)
            return NotFound("Fahrzeug nicht gefunden");

        var query = _context.VehicleAssignments
            .Include(va => va.User)
            .Where(va => va.VehicleId == vehicleId);

        if (!includeReturned)
        {
            query = query.Where(va => va.ReturnedAt == null);
        }

        var assignments = await query
            .OrderByDescending(va => va.AssignedAt)
            .Select(va => new
            {
                va.Id,
                va.UserId,
                User = new
                {
                    va.User.Id,
                    va.User.FirstName,
                    va.User.LastName,
                    va.User.Email
                },
                AssignmentType = va.AssignmentType.ToString(),
                va.AssignedAt,
                va.ReturnedAt,
                va.Notes,
                IsActive = va.ReturnedAt == null
            })
            .ToListAsync();

        return Ok(assignments);
    }

    [HttpGet("user/{userId}")]
    [SwaggerOperation(
        Summary = "Zuweisungen für einen Benutzer abrufen",
        Description = "Gibt alle Zuweisungen für einen bestimmten Benutzer zurück."
    )]
    [SwaggerResponse(200, "Liste der Zuweisungen")]
    public async Task<IActionResult> GetByUser(Guid userId, [FromQuery] bool includeReturned = false)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return NotFound("Benutzer nicht gefunden");

        var query = _context.VehicleAssignments
            .Include(va => va.Vehicle)
            .Where(va => va.UserId == userId && va.Vehicle.TenantId == tenantId);

        if (!includeReturned)
        {
            query = query.Where(va => va.ReturnedAt == null);
        }

        var assignments = await query
            .OrderByDescending(va => va.AssignedAt)
            .Select(va => new
            {
                va.Id,
                va.VehicleId,
                Vehicle = new
                {
                    va.Vehicle.Id,
                    va.Vehicle.LicensePlate,
                    va.Vehicle.Make,
                    va.Vehicle.Model,
                    va.Vehicle.Year,
                    Type = va.Vehicle.Type.ToString()
                },
                AssignmentType = va.AssignmentType.ToString(),
                va.AssignedAt,
                va.ReturnedAt,
                va.Notes,
                IsActive = va.ReturnedAt == null
            })
            .ToListAsync();

        return Ok(assignments);
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Neue Fahrzeugzuweisung erstellen",
        Description = "Weist ein Fahrzeug einem Benutzer zu (permanent für Dienstwagen, temporär für Poolfahrzeuge)."
    )]
    [SwaggerResponse(201, "Zuweisung erstellt")]
    [SwaggerResponse(400, "Ungültige Anfrage")]
    [SwaggerResponse(409, "Fahrzeug ist bereits zugewiesen")]
    public async Task<IActionResult> Create([FromBody] CreateVehicleAssignmentDto dto)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        // Validate vehicle exists and belongs to tenant
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == dto.VehicleId && v.TenantId == tenantId && v.IsActive);

        if (vehicle == null)
            return NotFound("Fahrzeug nicht gefunden oder nicht aktiv");

        // Validate user exists
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.UserId);

        if (user == null)
            return NotFound("Benutzer nicht gefunden");

        // Check if vehicle is already assigned
        var existingAssignment = await _context.VehicleAssignments
            .FirstOrDefaultAsync(va => va.VehicleId == dto.VehicleId && va.ReturnedAt == null);

        if (existingAssignment != null)
            return Conflict(new { error = "Fahrzeug ist bereits zugewiesen", existingAssignmentId = existingAssignment.Id });

        // Parse assignment type
        if (!Enum.TryParse<VehicleAssignmentType>(dto.AssignmentType, out var assignmentType))
            return BadRequest("Ungültiger Zuweisungstyp. Erlaubt: Permanent, Temporary, Reservation");

        var assignment = new VehicleAssignment
        {
            Id = Guid.NewGuid(),
            VehicleId = dto.VehicleId,
            UserId = dto.UserId,
            AssignmentType = assignmentType,
            AssignedAt = DateTime.UtcNow,
            Notes = dto.Notes
        };

        _context.VehicleAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Vehicle {VehicleId} assigned to user {UserId} as {AssignmentType}",
            dto.VehicleId, dto.UserId, assignmentType);

        return CreatedAtAction(
            nameof(GetById),
            new { id = assignment.Id },
            new
            {
                assignment.Id,
                assignment.VehicleId,
                assignment.UserId,
                AssignmentType = assignment.AssignmentType.ToString(),
                assignment.AssignedAt,
                assignment.Notes
            });
    }

    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Fahrzeugzuweisung aktualisieren",
        Description = "Aktualisiert Notizen oder den Zuweisungstyp einer bestehenden Zuweisung."
    )]
    [SwaggerResponse(200, "Zuweisung aktualisiert")]
    [SwaggerResponse(404, "Zuweisung nicht gefunden")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVehicleAssignmentDto dto)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var assignment = await _context.VehicleAssignments
            .Include(va => va.Vehicle)
            .FirstOrDefaultAsync(va => va.Id == id && va.Vehicle.TenantId == tenantId);

        if (assignment == null)
            return NotFound("Fahrzeugzuweisung nicht gefunden");

        if (!string.IsNullOrEmpty(dto.AssignmentType))
        {
            if (Enum.TryParse<VehicleAssignmentType>(dto.AssignmentType, out var assignmentType))
            {
                assignment.AssignmentType = assignmentType;
            }
            else
            {
                return BadRequest("Ungültiger Zuweisungstyp");
            }
        }

        if (dto.Notes != null)
        {
            assignment.Notes = dto.Notes;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Vehicle assignment {AssignmentId} updated", id);

        return Ok(new
        {
            assignment.Id,
            assignment.VehicleId,
            assignment.UserId,
            AssignmentType = assignment.AssignmentType.ToString(),
            assignment.AssignedAt,
            assignment.ReturnedAt,
            assignment.Notes
        });
    }

    [HttpPost("{id}/return")]
    [SwaggerOperation(
        Summary = "Fahrzeug zurückgeben",
        Description = "Markiert eine Fahrzeugzuweisung als beendet (ReturnedAt wird gesetzt)."
    )]
    [SwaggerResponse(200, "Fahrzeug zurückgegeben")]
    [SwaggerResponse(404, "Zuweisung nicht gefunden")]
    [SwaggerResponse(409, "Fahrzeug wurde bereits zurückgegeben")]
    public async Task<IActionResult> ReturnVehicle(Guid id)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var assignment = await _context.VehicleAssignments
            .Include(va => va.Vehicle)
            .FirstOrDefaultAsync(va => va.Id == id && va.Vehicle.TenantId == tenantId);

        if (assignment == null)
            return NotFound("Fahrzeugzuweisung nicht gefunden");

        if (assignment.ReturnedAt != null)
            return Conflict("Fahrzeug wurde bereits zurückgegeben");

        assignment.ReturnedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Vehicle {VehicleId} returned by user {UserId}",
            assignment.VehicleId, assignment.UserId);

        return Ok(new
        {
            assignment.Id,
            assignment.VehicleId,
            assignment.UserId,
            assignment.AssignedAt,
            assignment.ReturnedAt,
            Message = "Fahrzeug erfolgreich zurückgegeben"
        });
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Fahrzeugzuweisung löschen",
        Description = "Löscht eine Fahrzeugzuweisung dauerhaft. Nur möglich, wenn keine Ladevorgänge existieren."
    )]
    [SwaggerResponse(204, "Zuweisung gelöscht")]
    [SwaggerResponse(404, "Zuweisung nicht gefunden")]
    [SwaggerResponse(409, "Zuweisung kann nicht gelöscht werden")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var assignment = await _context.VehicleAssignments
            .Include(va => va.Vehicle)
            .FirstOrDefaultAsync(va => va.Id == id && va.Vehicle.TenantId == tenantId);

        if (assignment == null)
            return NotFound("Fahrzeugzuweisung nicht gefunden");

        // Check if there are any charging sessions associated with this assignment
        var hasChargingSessions = await _context.ChargingSessions
            .AnyAsync(cs => cs.VehicleId == assignment.VehicleId && 
                           cs.UserId == assignment.UserId &&
                           cs.StartedAt >= assignment.AssignedAt &&
                           (assignment.ReturnedAt == null || cs.StartedAt <= assignment.ReturnedAt));

        if (hasChargingSessions)
        {
            return Conflict("Zuweisung kann nicht gelöscht werden, da bereits Ladevorgänge existieren. " +
                          "Stattdessen können Sie das Fahrzeug zurückgeben.");
        }

        _context.VehicleAssignments.Remove(assignment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Vehicle assignment {AssignmentId} deleted", id);

        return NoContent();
    }
}

public record CreateVehicleAssignmentDto(
    Guid VehicleId,
    Guid UserId,
    string AssignmentType, // "Permanent", "Temporary", "Reservation"
    string? Notes
);

public record UpdateVehicleAssignmentDto(
    string? AssignmentType,
    string? Notes
);

