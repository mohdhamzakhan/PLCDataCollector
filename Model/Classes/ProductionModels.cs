using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PLCDataCollector.Model.Classes
{
    public class ProductionData
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string LineId { get; set; }

        public int? ScheduleId { get; set; }

        [Required]
        public int ShiftId { get; set; }

        [Required]
        public string ProductCode { get; set; }

        [Required]
        public string PartNumber { get; set; }

        [Required]
        public string ShiftName { get; set; }

        [Required]
        public int ActualCount { get; set; }

        [Required]
        public int PlannedCount { get; set; }

        [Required]
        public int GoodQuantity { get; set; }

        [Required]
        public int ScrapQuantity { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public ProductionStatus Status { get; set; }

        public decimal CycleTime { get; set; }

        public decimal Efficiency { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Navigation properties
        public LineDetail LineDetail { get; set; }
        public Shifts Shift { get; set; }
        public virtual ProductionSchedule Schedule { get; set; }
        public ICollection<QualityChecks> QualityChecks { get; set; }
    }

    public class ProductionPlan
    {
        public int Id { get; set; }
        public string LineId { get; set; }
        public string PartNumber { get; set; }
        public int PlannedQuantity { get; set; }
        public DateTime PlannedDate { get; set; }
        public string Shift { get; set; }
        public int StandardCycleTime { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ShiftStatus
    {
        public string? CurrentShift { get; set; }
        public DateTime ShiftStartTime { get; set; }
        public DateTime ShiftEndTime { get; set; }
        public TimeSpan TimeRemaining { get; set; }
        public int ActualProduction { get; set; }
        public int PlannedProduction { get; set; }
        public decimal EfficiencyPercentage { get; set; }
        public ProductionStatus Status { get; set; }
        public List<ProductionAlert> Alerts { get; set; } = new();
    }

    public class ProductionAlert
    {
        public string Type { get; set; } // "BehindSchedule", "AheadSchedule", "Error"
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public AlertSeverity Severity { get; set; }
    }
    public class PLCData
    {
        public string LineId { get; set; }
        public DateTime Timestamp { get; set; }
        public int CurrentCount { get; set; }
        public string PartNumber { get; set; }
        public int CycleTime { get; set; }
        public bool IsRunning { get; set; }
        public Dictionary<string, object> RawData { get; set; } = new();
    }
    public enum ProductionStatus
    {
        Idle,
        Running,
        Stopped,
        Maintenance,
        Error,
        Completed,
        Scheduled,
        Setup
    }

    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }

    public class RealTimeGraphData
    {
        public List<GraphPoint> ActualProduction { get; set; } = new();
        public List<GraphPoint> PlannedProduction { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public string CurrentShift { get; set; }
        public List<ShiftBoundary> ShiftBoundaries { get; set; } = new();
    }

    public class GraphPoint
    {
        public DateTime Timestamp { get; set; }
        public int Value { get; set; }
        public string? Label { get; set; }
    }

    public class ShiftBoundary
    {
        public DateTime Timestamp { get; set; }
        public string ShiftName { get; set; }
        public string Color { get; set; }
    }
}
