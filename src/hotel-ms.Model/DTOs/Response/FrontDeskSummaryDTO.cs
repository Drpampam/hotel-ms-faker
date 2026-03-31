namespace hotelier_core_app.Model.DTOs.Response
{
    public class FrontDeskSummaryDTO
    {
        public DateTime Date { get; set; }
        public int ExpectedArrivals { get; set; }
        public int ActualCheckIns { get; set; }
        public int ExpectedDepartures { get; set; }
        public int ActualCheckOuts { get; set; }
        public int CurrentlyOccupied { get; set; }
        public int PendingServiceRequests { get; set; }
        public int PendingHousekeepingTasks { get; set; }
    }
}
