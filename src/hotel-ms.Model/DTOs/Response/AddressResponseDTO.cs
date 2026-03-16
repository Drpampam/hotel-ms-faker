using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Model.DTOs.Response
{
    public class AddressResponseDTO
    {
        public long Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
    }
}
