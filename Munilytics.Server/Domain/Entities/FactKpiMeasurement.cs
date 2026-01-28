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
    }
}
