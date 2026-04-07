using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class AddReservationExpenseDTO
    {
        [Required]
        [StringLength(255)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Category { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }
    }
}
