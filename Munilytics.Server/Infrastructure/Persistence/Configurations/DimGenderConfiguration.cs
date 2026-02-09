using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Munilytics.Server.Domain.Entities;
using Munilytics.Server.Domain.Enums;

namespace Munilytics.Server.Infrastructure.Persistence.Configurations
{
    public class DimGenderConfiguration : IEntityTypeConfiguration<DimGender>
    {
        public void Configure(EntityTypeBuilder<DimGender> builder)
        {
            builder.HasKey(dg => dg.Id);

            builder.Property(dg => dg.Code).HasMaxLength(1).HasConversion<string>().IsRequired(); // Makes certain that the db saves it as a char and not the index of the enum, ergo T instead of 0, for readability.
            builder.Property(dg => dg.GenderName).IsRequired();
            builder.Property(dg => dg.SortOrder).IsRequired();

            builder
                .HasMany(dg => dg.FactKpiMeasurements)
                .WithOne(fkm => fkm.DimGender)
                .HasForeignKey(fkm => fkm.DimGenderId);

            builder.HasData(
                new DimGender { Id = 1, Code = GenderCode.T, GenderName = "Total", SortOrder = 1 },
                new DimGender { Id = 2, Code = GenderCode.K, GenderName = "Female", SortOrder = 2 },
                new DimGender { Id = 3, Code = GenderCode.M, GenderName = "Male", SortOrder = 3 }
            );
        }
    }
}
