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

            builder.Property(dk => dk.Title).IsRequired();
            builder.Property(dk => dk.Description).IsRequired();
            builder.Property(dk => dk.IsDividedByGender).IsRequired();
            builder.Property(dk => dk.MunicipalityType).IsRequired();
            builder.Property(dk => dk.Auspice);
            builder.Property(dk => dk.OperatingArea).IsRequired();
            builder.Property(dk => dk.Perspective).IsRequired();
            builder.Property(dk => dk.Unit).IsRequired();

            builder
                .HasMany(dk => dk.FactKpiMeasurements)
                .WithOne(fkm => fkm.DimKpi)
                .HasForeignKey(fkm => fkm.DimKpiId);
        }
    }
}
