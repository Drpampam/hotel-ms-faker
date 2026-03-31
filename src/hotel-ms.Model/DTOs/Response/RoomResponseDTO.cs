using hotelier_core_app.Core.States;

namespace hotelier_core_app.Model.DTOs.Response
{
    public class RoomResponseDTO
    {
        public long Id { get; set; }
        public string? Number { get; set; }
        public string? Type { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; }
        public RoomState RoomState { get; set; }
        public long PropertyId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}
