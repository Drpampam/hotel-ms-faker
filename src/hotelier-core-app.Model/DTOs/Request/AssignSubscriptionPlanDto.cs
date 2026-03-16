using hotelier_core_app.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request;

public class AssignSubscriptionPlanDTO
{
    /// <summary>
    /// Gets or sets the tenant ID to assign the subscription plan to.
    /// </summary>
    [Required]
    public long TenantId { get; set; }

    /// <summary>
    /// Gets or sets the subscription plan to assign.
    /// </summary>
    [Required]
    [EnumDataType(typeof(Subscription), ErrorMessage = "Invalid subscription plan.")]
    public Subscription SubscriptionPlan { get; set; }

    /// <summary>
    /// Gets or sets the number of months for the subscription plan.
    /// </summary>
    [Required]
    public int NumberOfMonths { get; set; }
}