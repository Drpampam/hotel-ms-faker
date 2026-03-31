namespace hotelier_core_app.Model.DTOs.Request
{
    public class AccruePointsRequestDTO
    {
        public int Points { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
