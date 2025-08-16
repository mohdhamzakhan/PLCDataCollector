namespace PLCDataCollector.Model.Classes
{
    public class AlarmDefinitions
    {
        public int Id { get; set; }
        public string LineId { get; set; }
        public int TagId { get; set; }
        public string AlarmType { get; set; }
        public double Threshold { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public int IsEnabled { get; set; } = 1;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public virtual LineDetail LineDetail { get; set; }
        public virtual Tag Tag { get; set; }
        public virtual ICollection<AlarmHistory> AlarmHistories { get; set; } = new List<AlarmHistory>();
    }
}
