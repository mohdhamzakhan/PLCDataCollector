namespace PLCDataCollector.Model.Classes
{
    public class AlarmHistory
    {
        public int Id { get; set; }
        public int AlarmDefinitionId { get; set; }
        public string LineId { get; set; }
        public double TagValue { get; set; }
        public DateTime TriggeredAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string AcknowledgedBy { get; set; }
        public DateTime? ClearedAt { get; set; }
        public string Status { get; set; }
        public virtual AlarmDefinitions AlarmDefinitions { get; set; }
        public virtual LineDetail LineDetail { get; set; }
    }
}
