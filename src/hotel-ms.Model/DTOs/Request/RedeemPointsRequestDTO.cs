namespace hotelier_core_app.Model.DTOs.Request
{
    public class RedeemPointsRequestDTO
    {
        public int Points { get; set; }
        public long ReservationId { get; set; }
    }
}
