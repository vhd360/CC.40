using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingControlSystem.Data.Entities;

public class VehicleAssignment
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid VehicleId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public VehicleAssignmentType AssignmentType { get; set; }

    [Required]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReturnedAt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Vehicle Vehicle { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public enum VehicleAssignmentType
{
    Permanent,  // Dienstwagen
    Temporary,  // Poolfahrzeug temporär zugewiesen
    Reservation // Reserviert
}
