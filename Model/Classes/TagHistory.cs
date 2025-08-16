namespace PLCDataCollector.Model.Classes
{
    public class TagHistory
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        public string LineId { get; set; }
        public string Value { get; set; }
        public string Quality { get; set; }
        public DateTime Timestamp { get; set; }
        public virtual Tag Tag { get; set; }
        public virtual LineDetail LineDetail { get; set; }
    }
}
