namespace Munilytics.Server.Domain.Entities
{
    public class FactKpiMeasurement
    {
        public int Id { get; set; }

        //Foreign Keys to all Dimension Tables
        public int DimMunicipality { get; set; }
        public int DimKPI { get; set; }
        public int DimGender { get; set; }
        public int DimTime { get; set; }


        public decimal Value { get; set; }
        public bool IsDefinite { get; set; }
        public int Count { get; set; }
        public DateTime ImportDate { get; set; }
        public DateTime LatestUpdate { get; set; }

        public int DimMunicipalityId { get; set; }
        public int DimKpiId { get; set; }
        public int DimGenderId { get; set; }
        public int DimTimeId { get; set; }

        public DimMunicipality DimMunicipality { get; set; } = null!;
        public DimKpi DimKpi { get; set; } = null!;
        public DimGender DimGender { get; set; } = null!;
        public DimTime DimTime { get; set; } = null!;
    }
}
