namespace PLCDataCollector.Model.Classes
{
    public class Shifts
    {
        public int Id { get; set; }
        public string LineId { get; set; }
        public string ShiftName { get; set; }
        public string StartTime { get; set; } // Stored as TEXT in DB
        public string EndTime { get; set; }
        public int IsActive { get; set; } = 1;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual LineDetail LineDetail { get; set; }
        public virtual ICollection<ProductionSchedule> ProductionSchedules { get; set; } = new List<ProductionSchedule>();
        public virtual ICollection<ProductionData> ProductionData { get; set; } = new List<ProductionData>();
        public virtual ICollection<Downtime> Downtimes { get; set; } = new List<Downtime>();
        public virtual ICollection<QualityChecks> QualityChecks { get; set; } = new List<QualityChecks>();
    }
}
