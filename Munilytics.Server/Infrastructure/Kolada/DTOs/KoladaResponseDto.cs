using System.Text.Json.Serialization;

namespace Munilytics.Server.Models.DTOs
{
    /// <summary>
    /// Generic DTO that can be used for almost all Kolada requests, since almost all data is
    /// wrapped in "value" objects.
    /// </summary>
    public record KoladaResponseDto<T>(
        [property: JsonPropertyName("values")] List<T> Values,
        [property: JsonPropertyName("next_url")] string? NextUrl
    );
}
