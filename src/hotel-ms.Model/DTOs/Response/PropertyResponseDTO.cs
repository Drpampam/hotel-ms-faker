using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Model.DTOs.Response
{
    public class PropertyResponseDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }

        public DateTime? CreationDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        public bool IsDeleted { get; set; }

        public Address Address { get; set; }
    }
}
