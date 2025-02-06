using Newtonsoft.Json;
using Athena.Models.App;

namespace Athena.Models.API.Athena;

public class AthenaRelease
{
    [JsonConverter(typeof(AthenaVersionConverter))]
    public AthenaVersion Version;

    public string DownloadUrl;
    public double UpdateSize;
    public string Changelog;
    public DateTime ReleaseDate;
}
