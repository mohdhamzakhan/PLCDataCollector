using Microsoft.EntityFrameworkCore;
using PLCDataCollector.Model.Classes;
using PLCDataCollector.Model.Database;

namespace PLCDataCollector.Service.Interfaces
{
    public interface ISourceDatabaseContext
    {
        DbSet<PlcData> PlcData { get; set; }
        DbSet<LineDetail> LineDetails { get; set; }
        DbSet<ConfigurationSetting> ConfigurationSettings { get; set; }
        DbSet<Tag> Tags { get; set; }
        DbSet<AlarmDefinitions> AlarmDefinitions { get; set; }
        DbSet<AlarmHistory> AlarmHistory { get; set; }
        DbSet<TagHistory> TagHistory { get; set; }
        DbSet<Shifts> Shifts { get; set; }
        DbSet<ProductionSchedule> ProductionSchedules { get; set; }
        DbSet<ProductionData> ProductionData { get; set; }
        DbSet<Downtime> Downtimes { get; set; }
        DbSet<QualityChecks> QualityChecks { get; set; }
        DbSet<MaintenanceLogs> MaintenanceLogs { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
