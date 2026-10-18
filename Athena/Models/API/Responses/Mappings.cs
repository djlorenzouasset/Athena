using System.Text.Json.Serialization;

namespace Athena.Models.API.Responses;

public class Mappings
{
    public string Url;
    public string FileName;
}

public class UedbMappingResponse
{
    public string Version;

    [JsonPropertyName("mappings")]
    public Dictionary<string, string> MappingsList { get; set; } = new();
}