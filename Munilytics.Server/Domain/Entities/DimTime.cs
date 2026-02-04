namespace Munilytics.Server.Domain.Entities
{
    public class DimTime
    {
        public int Id { get; set; } 
        public int Year { get; set; } 
        public int Decade { get; set; } 
        public string MandatePeriod { get; set; } = string.Empty;
        public bool IsElectionYear { get; set; } 
        public int RelativeYear { get; set; } //Timespan

        public ICollection<FactKpiMeasurement> FactKpiMeasurements { get; set; } = new List<FactKpiMeasurement>();
    }
}
