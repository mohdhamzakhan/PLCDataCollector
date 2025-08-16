// Create a temporary simple DbContext for testing
using Microsoft.EntityFrameworkCore;
using PLCDataCollector.Model.Classes;
using PLCDataCollector.Test;

namespace PLCDataCollector.Test
{
    public class SimpleTestContext : DbContext
    {
        public SimpleTestContext(DbContextOptions<SimpleTestContext> options)
            : base(options)
        {
        }

        public DbSet<PlcData> PlcData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Very basic configuration
            modelBuilder.Entity<PlcData>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LineId).IsRequired();
            });
        }
    }
}

