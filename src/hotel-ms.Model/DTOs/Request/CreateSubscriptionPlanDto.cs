using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request;

public class CreateSubscriptionPlanDTO
{
    /// <summary>
    /// Gets or sets the name of the subscription plan.
    /// </summary>
    [Required, StringLength(50)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the subscription plan.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the price of the subscription plan.
    /// </summary>
    [Required]
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the discount ID associated with the subscription plan, if any.
    /// </summary>
    public long? DiscountId { get; set; }
}