namespace hotelier_core_app.Model.DTOs.Response
{
    public class GuestProfileResponseDTO
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PassportNumber { get; set; }
        public string? Nationality { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PreferredRoomType { get; set; }
        public string? SpecialRequests { get; set; }
        public int LoyaltyPoints { get; set; }
        public string? LoyaltyTier { get; set; }
        public long? TenantId { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
