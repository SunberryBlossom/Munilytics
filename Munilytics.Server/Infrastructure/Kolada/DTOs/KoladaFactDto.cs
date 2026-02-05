using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Munilytics.Server.Infrastructure.Kolada.DTOs
{
    public record KoladaFactDto(
        [property: JsonPropertyName("gender")] string Gender,
        [property: JsonPropertyName("count")] string Count,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("value")] decimal Value,
        [property: JsonPropertyName("isdeleted")] bool Isdeleted,
        [property: JsonPropertyName("kpi")] string KpiId,
        [property: JsonPropertyName("period")] string Period,
        [property: JsonPropertyName("municipality")] string MunicipalityId
    );
}