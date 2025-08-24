using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PLCDataCollector.Model.Classes
{
    public class LineDetail
    {
        public int Id { get; set; }
        [Required]
        public string LineId { get; set; }
        [Required]
        public string LineName { get; set; }
        public string LineType { get; set; }
        public string Description { get; set; }
        public int IsActive { get; set; } = 1;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Updated to match JSON structure
        [Required]
        public DataLocationConfig Data_Location { get; set; }

        // Option 2: Add separate property for configuration object binding
        [NotMapped]
        public DataLocationConfig DataLocationConfig { get; set; }

        // Updated to match JSON structure
        public PLCConfig PLC { get; set; }

        [Required]
        public ShiftConfigurationDetail ShiftConfiguration { get; set; }

        // Add GraphSettings to match JSON
        public LineGraphSettings GraphSettings { get; set; }

        // Keep backward compatibility - not mapped to database
        [NotMapped]
        public PLCConfig PLCConfig { get; set; }
        
        public virtual ICollection<PlcData> PlcData { get; set; } = new List<PlcData>();
        public virtual ICollection<ConfigurationSetting> ConfigurationSettings { get; set; } = new List<ConfigurationSetting>();
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public virtual ICollection<AlarmDefinitions> AlarmDefinitions { get; set; } = new List<AlarmDefinitions>();
        public virtual ICollection<AlarmHistory> AlarmHistories { get; set; } = new List<AlarmHistory>();
        public virtual ICollection<TagHistory> TagHistories { get; set; } = new List<TagHistory>();
        public virtual ICollection<Shifts> Shifts { get; set; } = new List<Shifts>();
        public virtual ICollection<ProductionSchedule> ProductionSchedules { get; set; } = new List<ProductionSchedule>();
        public virtual ICollection<ProductionData> ProductionData { get; set; } = new List<ProductionData>();
        public virtual ICollection<Downtime> Downtimes { get; set; } = new List<Downtime>();
        public virtual ICollection<QualityChecks> QualityChecks { get; set; } = new List<QualityChecks>();
        public virtual ICollection<MaintenanceLogs> MaintenanceLogs { get; set; } = new List<MaintenanceLogs>();
    }
}