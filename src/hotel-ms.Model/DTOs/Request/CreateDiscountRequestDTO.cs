using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class CreateDiscountRequestDTO
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
        public decimal Percentage { get; set; }

        public decimal? FixedAmount { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? MinimumStayDays { get; set; }

        public int? MaximumStayDays { get; set; }

        public long? TenantId { get; set; }

        public long? PropertyId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
