using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Athena.Services;

public static class APIEndpoints
{
    private static readonly RestClient _client = new(new RestClientOptions
    {
        UserAgent = $"Athena/{Globals.Version}",
        Timeout = TimeSpan.FromMilliseconds(1000 * 5),
    }, configureSerialization: s => s.UseSerializer<JsonNetSerializer>());

    public static async Task<bool> DownloadFileAsync(string url, string path, bool bOverride = true)
    {
        var request = new RestRequest(url);
        var data = await _client.DownloadDataAsync(request);
        if (data is null) return false;

        if (!File.Exists(path) || bOverride)
        {
            var file = new FileInfo(path);
            await File.WriteAllBytesAsync(file.FullName, data);
        }

        return true;
    }
}