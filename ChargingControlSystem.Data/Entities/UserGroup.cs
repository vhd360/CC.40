using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingControlSystem.Data.Entities;

public class UserGroup
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Invite token for QR code self-joining
    [MaxLength(100)]
    public string? InviteToken { get; set; }

    public DateTime? InviteTokenExpiresAt { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ICollection<UserGroupMembership> UserGroupMemberships { get; set; } = new List<UserGroupMembership>();
    public virtual ICollection<GroupPermission> GroupPermissions { get; set; } = new List<GroupPermission>();
    public virtual ICollection<UserGroupChargingStationGroupPermission> ChargingStationGroupPermissions { get; set; } = new List<UserGroupChargingStationGroupPermission>();
}
