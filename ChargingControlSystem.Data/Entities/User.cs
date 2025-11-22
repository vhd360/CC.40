using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChargingControlSystem.Data.Enums;

namespace ChargingControlSystem.Data.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public bool IsEmailConfirmed { get; set; } = false;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ICollection<UserGroupMembership> UserGroupMemberships { get; set; } = new List<UserGroupMembership>();
    public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    public virtual ICollection<VehicleAssignment> VehicleAssignments { get; set; } = new List<VehicleAssignment>();
    public virtual ICollection<ChargingSession> ChargingSessions { get; set; } = new List<ChargingSession>();
    public virtual ICollection<QrCode> QrCodes { get; set; } = new List<QrCode>();
    public virtual ICollection<AuthorizationMethod> AuthorizationMethods { get; set; } = new List<AuthorizationMethod>();
    public virtual ICollection<ChargingStation> OwnedChargingStations { get; set; } = new List<ChargingStation>();
}
