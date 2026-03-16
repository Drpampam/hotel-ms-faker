using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Request
{
    public class AddPropertyRequestDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Image { get; set; }
        [Required]
        public long TenantId { get; set; }
        [Required]
        public CreateAddressRequestDTO Address { get; set; }
    }
}
