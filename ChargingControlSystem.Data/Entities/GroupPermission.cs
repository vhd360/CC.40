using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingControlSystem.Data.Entities;

public class GroupPermission
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserGroupId { get; set; }

    [Required]
    public Guid PermissionId { get; set; }

    [Required]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual UserGroup UserGroup { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}
