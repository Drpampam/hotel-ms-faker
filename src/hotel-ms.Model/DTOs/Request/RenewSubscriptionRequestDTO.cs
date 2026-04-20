using hotelier_core_app.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class RenewSubscriptionRequestDTO
    {
        [Required]
        public string Code { get; set; } = string.Empty;
    }

    public class AdminRenewRequestDTO
    {
        [Required]
        public PlanType PlanType { get; set; }
    }
}
