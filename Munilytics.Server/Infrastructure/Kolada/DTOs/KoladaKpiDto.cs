using System.Text.Json.Serialization;

namespace Munilytics.Server.Models.DTOs
{
    public record KoladaKpiDto(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("is_divided_by_gender")] bool IsDividedByGender,
        [property: JsonPropertyName("municipality_type")] string MunicipalityType,
        [property: JsonPropertyName("auspice")] string Auspice,
        [property: JsonPropertyName("operating_area")] string OperatingArea,
        [property: JsonPropertyName("perspective")] string Perspective
    );

}
