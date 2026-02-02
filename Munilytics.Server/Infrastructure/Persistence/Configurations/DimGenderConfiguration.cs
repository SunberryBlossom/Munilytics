using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munilytics.Server.Domain.Entities;

namespace Munilytics.Server.Infrastructure.Persistence.Configurations
{
    public class DimGenderConfiguration : IEntityTypeConfiguration<DimGender>
    {
        public void Configure(EntityTypeBuilder<DimGender> builder)
        {
            builder.HasKey(dg => dg.Code);

            builder.Property(dg => dg.GenderName).IsRequired();
            builder.Property(dg => dg.SortOrder).IsRequired();

            builder
                .HasMany(dg => dg.FactKpiMeasurements)
                .WithOne(fkm => fkm.DimGender)
                .HasForeignKey(fkm => fkm.DimGenderId);
        }
    }
}
