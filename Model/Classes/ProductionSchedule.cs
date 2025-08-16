namespace PLCDataCollector.Model.Classes
{
    public class ProductionSchedule
    {
        public int Id { get; set; }
        public string LineId { get; set; }
        public int ShiftId { get; set; }
        public string ProductCode { get; set; }
        public int PlannedQuantity { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = "Planned";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public LineDetail LineDetail { get; set; }
        public Shifts Shift { get; set; }
        public ICollection<ProductionData> ProductionData { get; set; }
    }
}
