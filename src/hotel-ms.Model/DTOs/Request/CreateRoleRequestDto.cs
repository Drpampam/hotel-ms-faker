using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request;

public class CreateRoleRequestDTO
{
    /// <summary>
    /// Gets or sets the name of the role to create.
    /// </summary>
    [Required, StringLength(255)]
    public string? RoleName { get; set; }

    /// <summary>
    /// Gets or sets the tenant ID associated with the role.
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the list of policy group IDs to assign to this role.
    /// </summary>
    public List<long> PolicyGroupIds { get; set; } = new List<long>();
}