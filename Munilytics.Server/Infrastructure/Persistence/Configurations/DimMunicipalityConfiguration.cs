using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munilytics.Server.Domain.Entities;

namespace Munilytics.Server.Infrastructure.Persistence.Configurations
{
    public class DimMunicipalityConfiguration :IEntityTypeConfiguration<DimMunicipality>
    {
        public void Configure(EntityTypeBuilder<DimMunicipality> builder)
        {
            builder.HasKey(dm => dm.Id);

            builder.Property(dm => dm.Title).HasMaxLength(200).IsRequired();
            builder.Property(dm => dm.Type).HasMaxLength(200).IsRequired();
            builder.Property(dm => dm.County).HasMaxLength(200).IsRequired();
            builder.Property(dm => dm.SkrGroupCode).HasMaxLength(200).IsRequired();
            builder.Property(dm => dm.SkrGroupName).HasMaxLength(200).IsRequired();

            builder
                .HasMany(dm => dm.FactKpiMeasurements)
                .WithOne(fkm => fkm.DimMunicipality)
                .HasForeignKey(fkm => fkm.DimMunicipalityId);
        }
    }
}
