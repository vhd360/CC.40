using System.ComponentModel.DataAnnotations;

namespace ChargingControlSystem.Data.Entities;

public class UserGroupChargingStationGroupPermission
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserGroupId { get; set; }

    [Required]
    public Guid ChargingStationGroupId { get; set; }

    [Required]
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual UserGroup UserGroup { get; set; } = null!;
    public virtual ChargingStationGroup ChargingStationGroup { get; set; } = null!;
}

