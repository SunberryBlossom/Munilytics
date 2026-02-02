using Microsoft.EntityFrameworkCore;
using Munilytics.Server.Domain.Entities;

namespace Munilytics.Server.Infrastructure.Persistence
{
    public class MunilyticsDbContext : DbContext
    {
        public DbSet<FactKpiMeasurement> Fact_KpiMeasurements { get; set; }
        public DbSet<DimMunicipality> Dim_Municipalities { get; set; }
        public DbSet<DimKpi> Dim_KPIs { get; set; }
        public DbSet<DimGender> Dim_Gender { get; set; }
        public DbSet<DimTime> Dim_Time { get; set; }

        public MunilyticsDbContext(DbContextOptions<MunilyticsDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
            base.OnModelCreating(modelBuilder);

            // TODO: implement Fluent API
        }
    }
}
