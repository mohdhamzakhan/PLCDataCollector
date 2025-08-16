using System.ComponentModel.DataAnnotations;

namespace PLCDataCollector.Model.Classes
{
    public class PlcData
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string LineId { get; set; }
        
        public int CurrentCount { get; set; }
        
        public string PartNumber { get; set; }
        
        public double CycleTime { get; set; }
        public int SyncStatus { get; set; }

        public bool IsRunning { get; set; }
        
        public string RawData { get; set; }
        
        public DateTime Timestamp { get; set; }
        public virtual LineDetail LineDetail { get; set; }
    }
}
