using Munilytics.Server.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Munilytics.Server.Domain.Entities
{
    public class DimGender
    {
        public int Id { get; set; }
        public GenderCode Code { get; set; }
        public string GenderName { get; set; } = string.Empty;
        public int SortOrder { get; set; }

        public ICollection<FactKpiMeasurement> FactKpiMeasurements { get; set; } = new List<FactKpiMeasurement>();
    }
}
