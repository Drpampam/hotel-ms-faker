namespace hotelier_core_app.Model.DTOs.Response;

public class RoleResponseDTO
{
    /// <summary>
    /// Gets or sets the ID of the role.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who created the role.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who last modified the role.
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the creation date of the role.
    /// </summary>
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the last modified date of the role.
    /// </summary>
    public DateTime? LastModifiedDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the tenant ID associated with the role.
    /// </summary>
    public long? TenantId { get; set; }
}