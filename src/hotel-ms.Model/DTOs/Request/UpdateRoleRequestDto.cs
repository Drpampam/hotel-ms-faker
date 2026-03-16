namespace hotelier_core_app.Model.DTOs.Request;

public class UpdateRoleRequestDTO
{
    /// <summary>
    /// Gets or sets the ID of the role to update.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the new name for the role.
    /// </summary>
    public string? RoleName { get; set; }
}