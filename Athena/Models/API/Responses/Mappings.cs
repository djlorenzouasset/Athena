using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Athena.Models.API.Responses;

public class Mappings
{
    public string Url;
    public string FileName;
}

public class UedbMappingResponse
{
    public string Version;

    [JsonProperty("mappings")]
    public Dictionary<string, string> MappingsList { get; set; } = new();
}