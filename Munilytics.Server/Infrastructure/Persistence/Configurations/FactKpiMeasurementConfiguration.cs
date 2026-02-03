using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munilytics.Server.Domain.Entities;

namespace Munilytics.Server.Infrastructure.Persistence.Configurations
{
    public class FactKpiMeasurementConfiguration : IEntityTypeConfiguration<FactKpiMeasurement>
    {
        public void Configure(EntityTypeBuilder<FactKpiMeasurement> builder)
        {
            builder.HasKey(kpm => kpm.Id);

            builder.Property(fkm => fkm.Value).IsRequired();
            builder.Property(fkm => fkm.IsDefinite).IsRequired();
            builder.Property(fkm => fkm.Count).IsRequired();
            builder.Property(fkm => fkm.ImportDate).IsRequired();
            builder.Property(fkm => fkm.LatestUpdate).IsRequired();

            builder
                .HasOne(fkm => fkm.DimMunicipality)
                .WithMany(dm => dm.FactKpiMeasurements)
                .HasForeignKey(fkm => fkm.DimMunicipalityId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder
                .HasOne(fkm => fkm.DimKpi)
                .WithMany(dk => dk.FactKpiMeasurements)
                .HasForeignKey(fkm => fkm.DimKpiId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(fkm => fkm.DimGender)
                .WithMany(dg => dg.FactKpiMeasurements)
                .HasForeignKey(fkm => fkm.DimGenderId);

            builder
                .HasOne(fkm => fkm.DimTime)
                .WithMany(dt => dt.FactKpiMeasurements)
                .HasForeignKey(fkm => fkm.DimTimeId);
        }
    }
}
