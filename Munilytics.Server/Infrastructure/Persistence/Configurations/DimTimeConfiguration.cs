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

            builder.HasData(GenerateTimeData());


        }

        private IEnumerable<DimTime> GenerateTimeData()
        {
            var times = new List<DimTime>();
            int idCounter = 0;

            for (int i = 1994; i <= DateTime.Now.Year; i++)
            {
                idCounter++;

                int offset = (i - 1994) % 4;
                int mandateStart = i - offset;

                times.Add(new DimTime
                {
                    Id = idCounter,
                    Year = i,
                    Decade = (i / 10) * 10,
                    IsElectionYear = (i - 1994) % 4 == 0,
                    RelativeYear = i - DateTime.Now.Year,
                    MandatePeriod = $"{mandateStart}-{mandateStart + 3}"
                });
            }

            return times;
        }
    }
}
