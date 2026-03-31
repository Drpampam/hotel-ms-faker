namespace hotelier_core_app.Model.DTOs.Response
{
    public class ReservationStatsDTO
    {
        public int TotalReservations { get; set; }
        public int PendingReservations { get; set; }
        public int ConfirmedReservations { get; set; }
        public int CheckedInCount { get; set; }
        public int CheckedOutCount { get; set; }
        public int CancelledCount { get; set; }
        public int NoShowCount { get; set; }
        public double AverageStayDays { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
