namespace hotelier_core_app.Model.DTOs.Response
{
    public class LoyaltyResponseDTO
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
        public int PointsEarned { get; set; }
        public int PointsRedeemed { get; set; }
        public int PointsBalance { get; set; }
        public string? Tier { get; set; }
    }
}
