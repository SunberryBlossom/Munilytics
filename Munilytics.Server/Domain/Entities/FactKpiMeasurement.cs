namespace Munilytics.Server.Domain.Entities
{
    public class FactKpiMeasurement
    {
        public int Id { get; set; }
        public decimal Value { get; set; }
        public bool IsDefinite { get; set; }
        public int Count { get; set; }
        public DateTime ImportDate { get; set; }
        public DateTime LatestUpdate { get; set; }


        public int DimMunicipalityId { get; set; }
        public DimMunicipality DimMunicipality { get; set; } = null!;
        public int DimKpiId { get; set; }
        public DimKpi DimKpi { get; set; } = null!;
        public int DimGenderId { get; set; }
        public DimGender DimGender { get; set; } = null!;
        public int DimTimeId { get; set; }
        public DimTime DimTime { get; set; } = null!;
    }
}
