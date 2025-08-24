using Azure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;

namespace PLCDataCollector.Model.Classes
{
    public class AppSettings
    {
        public Logging Logging { get; set; } = new();
        public GraphSettings GraphSettings { get; set; } = new();
        public Serilog Serilog { get; set; } = new();
        public RealTimeSettings RealTimeSettings { get; set; } = new();
        public ConnectionStrings ConnectionStrings { get; set; } = new();
        public DatabaseSettings DatabaseSettings { get; set; } = new();
        public DataSyncSettings DataSync { get; set; } = new();
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

    // Updated ConnectionStrings to match JSON structure
    public class ConnectionStrings
    {
        public EnvironmentConnectionStrings SourceDatabase { get; set; } = new();
        public EnvironmentConnectionStrings TargetDatabase { get; set; } = new();

        // Keep backward compatibility
        public string Temporary { get; set; }
        public string Production { get; set; }
    }

    public class EnvironmentConnectionStrings
    {
        public string Development { get; set; }
        public string Production { get; set; }
    }

    // Add DatabaseSettings class
    public class DatabaseSettings
    {
        public EnvironmentDatabaseType SourceType { get; set; } = new();
    }

    public class EnvironmentDatabaseType
    {
        public string Development { get; set; } = "SQLite";
        public string Production { get; set; } = "Oracle";
    }

    public class ConfigurationSetting
    {
        public int Id { get; set; }

        [Required]
        public string LineId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SettingKey { get; set; }

        [Required]
        public string SettingValue { get; set; }

        [Required]
        [MaxLength(50)]
        public string DataType { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual LineDetail LineDetail { get; set; }
    }

    public class Tag
    {
        public int Id { get; set; }

        [Required]
        public string LineId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TagName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Address { get; set; }

        [Required]
        [MaxLength(50)]
        public string DataType { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int ScanRate { get; set; } = 1000;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual LineDetail LineDetail { get; set; }
        public virtual ICollection<AlarmDefinitions> AlarmDefinitions { get; set; } = new List<AlarmDefinitions>();
        public virtual ICollection<TagHistory> TagHistories { get; set; } = new List<TagHistory>();
    }

    public class PLCConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string FTP { get; set; }
        public string IP { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FilePath { get; set; }
        public int CycleTime { get; set; }
        public int SkipLine { get; set; }
    }

    public class DataLocationConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int Lenght { get; set; }
        public int Justifation { get; set; }
        public int Part { get; set; }
        public int Time { get; set; }
        public int Part_Number { get; set; }
    }

    public class ShiftConfigurationDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public ShiftDetail ShiftA { get; set; } = new();
        public ShiftDetail ShiftB { get; set; } = new();
        public ShiftDetail ShiftC { get; set; } = new();

        public LineDetail LineDetail { get; set; }
    }

    public class ShiftDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Color { get; set; }
        public List<string> BreakTimes { get; set; } = new();
    }

    public class LineGraphSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int RefreshInterval { get; set; } = 5000;
        public bool ShowCurrentTime { get; set; } = true;
        public string CurrentTimeColor { get; set; } = "#FF0000";
        public int GridInterval { get; set; } = 60;
        public bool ShowShiftBoundaries { get; set; } = true;
    }
}