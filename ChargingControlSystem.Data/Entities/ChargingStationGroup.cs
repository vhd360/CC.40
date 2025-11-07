using System.ComponentModel.DataAnnotations;

namespace ChargingControlSystem.Data.Entities;

public class ChargingStationGroup
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    // Optional: Gruppe kann einem bestimmten Ladepark zugeordnet sein
    public Guid? ChargingParkId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ChargingPark? ChargingPark { get; set; }
    public virtual ICollection<ChargingStationGroupMembership> StationMemberships { get; set; } = new List<ChargingStationGroupMembership>();
    public virtual ICollection<UserGroupChargingStationGroupPermission> UserGroupPermissions { get; set; } = new List<UserGroupChargingStationGroupPermission>();
}

public class ChargingStationGroupMembership
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ChargingStationGroupId { get; set; }

    [Required]
    public Guid ChargingStationId { get; set; }

    [Required]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ChargingStationGroup ChargingStationGroup { get; set; } = null!;
    public virtual ChargingStation ChargingStation { get; set; } = null!;
}

