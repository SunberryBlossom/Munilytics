using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Munilytics.Server.Infrastructure.Kolada.DTOs
{
    public record KoladaFactDto(
        [property: JsonPropertyName("gender")] string Gender,
        [property: JsonPropertyName("count")] int Count,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("value")] decimal Value,
        [property: JsonPropertyName("isdeleted")] bool Isdeleted,
        [property: JsonPropertyName("kpi")] string KpiKoladaId,
        [property: JsonPropertyName("period")] int Year,
        [property: JsonPropertyName("municipality")] string MunicipalityKoladaId
    );

    public record KoladaGroupResponse(
            [property: JsonPropertyName("kpi")] string Kpi,
            [property: JsonPropertyName("municipality")] string Municipality,
            [property: JsonPropertyName("period")] int Period,
            [property: JsonPropertyName("values")] List<KoladaPointResponse>? InnerValues
        );

    public record KoladaPointResponse(
        [property: JsonPropertyName("value")] decimal? Value,
        [property: JsonPropertyName("count")] int? Count,
        [property: JsonPropertyName("gender")] string? Gender,
        [property: JsonPropertyName("status")] string? Status
    );
}