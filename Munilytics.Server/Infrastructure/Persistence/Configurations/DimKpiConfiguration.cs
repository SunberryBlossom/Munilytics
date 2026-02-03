using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munilytics.Server.Domain.Entities;

namespace Munilytics.Server.Infrastructure.Persistence.Configurations
{
    public class DimKpiConfiguration : IEntityTypeConfiguration<DimKpi>
    {
        public void Configure(EntityTypeBuilder<DimKpi> builder)
        {
            builder.HasKey(dk => dk.Id);

            builder.Property(dk => dk.Title).HasMaxLength(400).IsRequired();
            builder.Property(dk => dk.Description).HasMaxLength(400).IsRequired();
            builder.Property(dk => dk.IsDividedByGender).IsRequired();
            builder.Property(dk => dk.MunicipalityType).IsRequired();
            builder.Property(dk => dk.Auspice).HasMaxLength(400).IsRequired();
            builder.Property(dk => dk.OperatingArea).HasMaxLength(400).IsRequired();
            builder.Property(dk => dk.Perspective).HasMaxLength(400).IsRequired();
            builder.Property(dk => dk.Unit).HasMaxLength(400).IsRequired();

            builder
                .HasMany(dk => dk.FactKpiMeasurements)
                .WithOne(fkm => fkm.DimKpi)
                .HasForeignKey(fkm => fkm.DimKpiId);
        }
    }
}
