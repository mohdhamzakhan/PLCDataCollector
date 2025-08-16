using Microsoft.EntityFrameworkCore;
using PLCDataCollector.Service.Interfaces;

namespace PLCDataCollector.Model.Classes
{
    
    public class PlcDataContext : DbContext
    {

        public PlcDataContext(DbContextOptions<PlcDataContext> options)
            : base(options)
        {
        }

        public DbSet<PlcData> PlcData { get; set; }
        public DbSet<LineDetail> LineDetails { get; set; }
        public DbSet<ConfigurationSetting> ConfigurationSettings { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<AlarmDefinitions> AlarmDefinitions { get; set; }
        public DbSet<AlarmHistory> AlarmHistory { get; set; }
        public DbSet<TagHistory> TagHistory { get; set; }
        public DbSet<Shifts> Shifts { get; set; }
        public DbSet<ProductionSchedule> ProductionSchedules { get; set; }
        public DbSet<ProductionData> ProductionData { get; set; }
        public DbSet<Downtime> Downtimes { get; set; }
        public DbSet<QualityChecks> QualityChecks { get; set; }
        public DbSet<MaintenanceLogs> MaintenanceLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure indexes
            modelBuilder.Entity<PlcData>()
                .HasIndex(p => new { p.LineId, p.SyncStatus });

            modelBuilder.Entity<LineDetail>()
                .HasIndex(l => l.LineId)
                .IsUnique();

            modelBuilder.Entity<ConfigurationSetting>()
                .HasIndex(c => new { c.LineId, c.SettingKey })
                .IsUnique();

            modelBuilder.Entity<Tag>()
                .HasIndex(t => new { t.LineId, t.TagName })
                .IsUnique();

            // Configure relationships
            modelBuilder.Entity<PlcData>()
                .HasOne(p => p.LineDetail)
                .WithMany(l => l.PlcData)
                .HasForeignKey(p => p.LineId)
                .HasPrincipalKey(l => l.LineId);

            modelBuilder.Entity<ConfigurationSetting>()
    .HasOne(c => c.LineDetail)
    .WithMany(l => l.ConfigurationSettings)
    .HasForeignKey(c => c.LineId)
    .HasPrincipalKey(l => l.LineId);
            modelBuilder.Entity<Tag>()
    .HasOne(t => t.LineDetail)
    .WithMany(l => l.Tags)
    .HasForeignKey(t => t.LineId)
    .HasPrincipalKey(l => l.LineId);
            modelBuilder.Entity<AlarmDefinitions>()
    .HasOne(a => a.LineDetail)
    .WithMany(l => l.AlarmDefinitions)
    .HasForeignKey(a => a.LineId)
    .HasPrincipalKey(l => l.LineId);

            modelBuilder.Entity<AlarmDefinitions>()
                .HasOne(a => a.Tag)
                .WithMany(t => t.AlarmDefinitions)
                .HasForeignKey(a => a.TagId);

            modelBuilder.Entity<AlarmHistory>()
    .HasOne(h => h.AlarmDefinitions)
    .WithMany(a => a.AlarmHistories)
    .HasForeignKey(h => h.AlarmDefinitionId);

            modelBuilder.Entity<AlarmHistory>()
                .HasOne(h => h.LineDetail)
                .WithMany(l => l.AlarmHistories)
                .HasForeignKey(h => h.LineId)
                .HasPrincipalKey(l => l.LineId);

            modelBuilder.Entity<TagHistory>()
    .HasOne(th => th.Tag)
    .WithMany(t => t.TagHistories)
    .HasForeignKey(th => th.TagId);

            modelBuilder.Entity<TagHistory>()
                .HasOne(th => th.LineDetail)
                .WithMany(l => l.TagHistories)
                .HasForeignKey(th => th.LineId)
                .HasPrincipalKey(l => l.LineId);

            modelBuilder.Entity<Shifts>()
    .HasOne(s => s.LineDetail)
    .WithMany(l => l.Shifts)
    .HasForeignKey(s => s.LineId)
    .HasPrincipalKey(l => l.LineId);

            modelBuilder.Entity<ProductionSchedule>()
    .HasOne(ps => ps.LineDetail)
    .WithMany(l => l.ProductionSchedules)
    .HasForeignKey(ps => ps.LineId)
    .HasPrincipalKey(l => l.LineId);

            modelBuilder.Entity<ProductionSchedule>()
                .HasOne(ps => ps.Shift)
                .WithMany(s => s.ProductionSchedules)
                .HasForeignKey(ps => ps.ShiftId);

            modelBuilder.Entity<ProductionData>()
    .HasOne(pd => pd.LineDetail)
    .WithMany(l => l.ProductionData)
    .HasForeignKey(pd => pd.LineId)
    .HasPrincipalKey(l => l.LineId);

            modelBuilder.Entity<ProductionData>()
                .HasOne(pd => pd.Schedule)
                .WithMany(ps => ps.ProductionData)
                .HasForeignKey(pd => pd.ScheduleId);

            modelBuilder.Entity<ProductionData>()
                .HasOne(pd => pd.Shift)
                .WithMany(s => s.ProductionData)
                .HasForeignKey(pd => pd.ShiftId);

            modelBuilder.Entity<Downtime>()
    .HasOne(d => d.LineDetail)
    .WithMany(l => l.Downtimes)
    .HasForeignKey(d => d.LineId)
    .HasPrincipalKey(l => l.LineId);

            modelBuilder.Entity<Downtime>()
                .HasOne(d => d.Shifts)
                .WithMany(s => s.Downtimes)
                .HasForeignKey(d => d.ShiftId);


            modelBuilder.Entity<QualityChecks>()
    .HasOne(q => q.LineDetail)
    .WithMany(l => l.QualityChecks)
    .HasForeignKey(q => q.LineId)
    .HasPrincipalKey(l => l.LineId);

            modelBuilder.Entity<QualityChecks>()
                .HasOne(q => q.ProductionData)
                .WithMany(pd => pd.QualityChecks)
                .HasForeignKey(q => q.ProductionDataId);

            modelBuilder.Entity<MaintenanceLogs>()
    .HasOne(m => m.LineDetail)
    .WithMany(l => l.MaintenanceLogs)
    .HasForeignKey(m => m.LineId)
    .HasPrincipalKey(l => l.LineId);





            // Add more relationship configurations...
        }
    }
}
