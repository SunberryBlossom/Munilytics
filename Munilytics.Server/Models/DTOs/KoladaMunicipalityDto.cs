using System.Text.Json.Serialization;

namespace Munilytics.Server.Models.DTOs
{
    public record KoladaMunicipalityDto(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("type")] string Type
    );
}
