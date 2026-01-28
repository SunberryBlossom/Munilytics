using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munilytics.Server.Domain.Entities;

namespace Munilytics.Server.Infrastructure.Persistence.Configurations
{
    public class DimTimeConfiguration : IEntityTypeConfiguration<DimTime>
    {
        public void Configure(EntityTypeBuilder<DimTime> builder)
        {
            builder.HasKey(dt => dt.Id);

            builder.Property(dt => dt.Year).IsRequired();
            builder.Property(dt => dt.Decade).IsRequired();
            builder.Property(dt => dt.MandatePeriod).HasMaxLength(9).IsRequired();
            builder.Property(dt => dt.IsElectionYear).IsRequired();
            builder.Property(dt => dt.RelativeYear).IsRequired();

            builder
                .HasMany(dt => dt.FactKpiMeasurements)
                .WithOne(fkm => fkm.DimTime)
                .HasForeignKey(fkm => fkm.DimTimeId);
        }
    }
}
