namespace hotelier_core_app.Model.DTOs.Response
{
    public class HousekeepingStatsDTO
    {
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int SkippedTasks { get; set; }
        public double CompletionRate { get; set; }
        public DateTime Date { get; set; }
    }
}
