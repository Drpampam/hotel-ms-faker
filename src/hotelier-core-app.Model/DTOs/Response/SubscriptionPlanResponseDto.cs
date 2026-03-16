namespace hotelier_core_app.Model.DTOs.Response;

public class SubscriptionPlanResponseDTO
{
    /// <summary>
    /// Gets or sets the ID of the subscription plan.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the subscription plan.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the subscription plan.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the price of the subscription plan.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the discount ID associated with the subscription plan, if any.
    /// </summary>
    public long? DiscountId { get; set; }

    /// <summary>
    /// Gets or sets the creation date of the subscription plan.
    /// </summary>
    public DateTime? CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the last modified date of the subscription plan.
    /// </summary>
    public DateTime? LastModifiedDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the subscription plan is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
}