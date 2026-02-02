namespace Munilytics.Server.Domain.Entities
{
    public class DimKpi
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsDividedByGender { get; set; }
        public string MunicipalityType { get; set; } = string.Empty;
        public string Auspice { get; set; } = string.Empty;
        public string OperatingArea { get; set; } = string.Empty;
        public string Perspective { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

        public ICollection<FactKpiMeasurement> FactKpiMeasurements { get; set; } = new List<FactKpiMeasurement>();       
    }
}