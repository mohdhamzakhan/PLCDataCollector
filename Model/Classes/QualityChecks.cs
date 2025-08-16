namespace PLCDataCollector.Model.Classes
{
    public class QualityChecks
    {
        public int Id { get; set; }
        public string LineId { get; set; }
        public int ProductionDataId { get; set; }
        public string CheckType { get; set; }
        public string Parameter { get; set; }
        public double Value { get; set; }
        public double Standard { get; set; }
        public string Status { get; set; }
        public DateTime CheckedAt { get; set; }
        public string CheckedBy { get; set; }
        public virtual LineDetail LineDetail { get; set; }
        public virtual Shifts Shifts { get; set; }
        public virtual ProductionData ProductionData { get; set; }
    }
}
