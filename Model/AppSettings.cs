namespace PLCDataCollector.Model
{
    public class AppSettings
    {
        public Logging Logging { get; set; } = new();
        public GraphSettings GraphSettings { get; set; } = new();
        public Serilog Serilog { get; set; } = new();
        public RealTimeSettings RealTimeSettings { get; set; } = new();
        public ConnectionStrings ConnectionStrings { get; set; } = new();
        public Dictionary<string, LineDetail> LineDetails { get; set; } = new();
        public string AllowedHosts { get; set; } = "*";
    }

    public class Logging
    {
        public Dictionary<string, string> LogLevel { get; set; } = new();
    }

    public class GraphSettings
    {
        public int MaxDataPoints { get; set; } = 100;
        public int TimeWindow { get; set; } = 300;
        public List<string> Colors { get; set; } = new();
        public bool GridLines { get; set; } = true;
        public bool ShowLegend { get; set; } = true;
    }

    public class Serilog
    {
        public List<string> Using { get; set; } = new();
        public string MinimumLevel { get; set; } = "Information";
        public List<WriteTo> WriteTo { get; set; } = new();
    }

    public class WriteTo
    {
        public string Name { get; set; }
        public FileArgs Args { get; set; }
    }

    public class FileArgs
    {
        public string Path { get; set; }
        public string RollingInterval { get; set; }
        public int RetainedFileCountLimit { get; set; }
    }

    public class RealTimeSettings
    {
        public int UpdateFrequency { get; set; } = 5;
        public string CurrentTimeZone { get; set; } = "IST";
        public bool ShowLiveMetrics { get; set; } = true;
        public AlertThresholds AlertThresholds { get; set; } = new();
    }

    public class AlertThresholds
    {
        public int BehindSchedule { get; set; } = 5;
        public int AheadSchedule { get; set; } = 10;
    }

    public class ConnectionStrings
    {
        public string Temporary { get; set; }
        public string Production { get; set; }
    }

    public class LineDetail
    {
        public string LineName { get; set; }
        public int LineId { get; set; }
        public string LineType { get; set; }
        public PLCConfig PLC { get; set; } = new();
        public DataLocationConfig Data_Location { get; set; } = new();
        public ShiftConfigurationDetail ShiftConfiguration { get; set; } = new();
        public LineGraphSettings GraphSettings { get; set; } = new();
    }

    public class PLCConfig
    {
        public string FTP { get; set; }
        public string IP { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FilePath { get; set; }
    }

    public class DataLocationConfig
    {
        public int Lenght { get; set; }
        public int Justifation { get; set; }
        public int Part { get; set; }
        public int Time { get; set; }
        public int Part_Number { get; set; }
    }

    public class ShiftConfigurationDetail
    {
        public ShiftDetail ShiftA { get; set; } = new();
        public ShiftDetail ShiftB { get; set; } = new();
        public ShiftDetail ShiftC { get; set; } = new();
    }

    public class ShiftDetail
    {
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Color { get; set; }
        public List<string> BreakTimes { get; set; } = new();
    }

    public class LineGraphSettings
    {
        public int RefreshInterval { get; set; } = 5000;
        public bool ShowCurrentTime { get; set; } = true;
        public string CurrentTimeColor { get; set; } = "#FF0000";
        public int GridInterval { get; set; } = 60;
        public bool ShowShiftBoundaries { get; set; } = true;
    }
}
