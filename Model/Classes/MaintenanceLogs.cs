namespace PLCDataCollector.Model.Classes
{
    public class MaintenanceLogs
    {
        public int Id { get; set; }
        public string LineId { get; set; }
        public string MaintenanceType { get; set; }
        public string Description { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; }
        public string Technician { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public virtual LineDetail LineDetail { get; set; }
    }
}
