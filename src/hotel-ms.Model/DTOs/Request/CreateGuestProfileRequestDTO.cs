using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class CreateGuestProfileRequestDTO
    {
        [Required]
        public long UserId { get; set; }

        [StringLength(100)]
        public string? PassportNumber { get; set; }

        [StringLength(100)]
        public string? Nationality { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(100)]
        public string? PreferredRoomType { get; set; }

        [StringLength(500)]
        public string? SpecialRequests { get; set; }

        public long? TenantId { get; set; }
    }
}
