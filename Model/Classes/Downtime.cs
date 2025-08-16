namespace PLCDataCollector.Model.Classes
{
    public class Downtime
    {
        public int Id { get; set; }
        public string LineId { get; set; }
        public int ShiftId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Duration { get; set; }
        public string Reason { get; set; }
        public string Category { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public virtual LineDetail LineDetail { get; set; }
        public virtual Shifts Shifts { get; set; }
    }
}
