namespace PLCDataCollector.Model
{
    public class ProductionData
    {
        public int Id { get; set; }
        public string LineId { get; set; }
        public DateTime Timestamp { get; set; }
        public int ActualCount { get; set; }
        public int PlannedCount { get; set; }
        public int CycleTime { get; set; } // in seconds
        public string PartNumber { get; set; }
        public string ShiftName { get; set; }
        public double Efficiency { get; set; }
        public ProductionStatus Status { get; set; }
        public string? Remarks { get; set; }
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
        public double EfficiencyPercentage { get; set; }
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
        Running,
        Idle,
        Break,
        Maintenance,
        Error
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
