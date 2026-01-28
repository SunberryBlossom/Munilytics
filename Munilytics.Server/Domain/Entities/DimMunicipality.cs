namespace Munilytics.Server.Domain.Entities
{
    public class DimMunicipality
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;
        public string SKRGroupCode { get; set; } = string.Empty;
        public string SKRGroupName { get; set; } = string.Empty;

        public ICollection<FactKpiMeasurement> FactKpiMeasurements { get; set; } = new List<FactKpiMeasurement>();
    }
}
