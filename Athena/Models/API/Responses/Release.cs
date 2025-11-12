using Newtonsoft.Json;

namespace Athena.Models.API.Responses;

public class AthenaRelease
{
    [JsonConverter(typeof(AthenaVersionConverter))] 
    public AthenaVersion Version;
    public string Changelog;
    public bool Required;
    public float UpdateSize;
    public string DownloadUrl;
    public DateTime ReleaseDate;
}
