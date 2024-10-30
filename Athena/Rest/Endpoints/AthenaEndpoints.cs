using RestSharp;
using Athena.Models;

namespace Athena.Rest.Endpoints;

public class AthenaEndpoints(RestClient client) : RestBase(client)
{
    public async Task<bool> DownloadFileAsync(string url, string path)
    {
        var request = new RestRequest(url);
        var data = await _client.DownloadDataAsync(request);
        if (data is not null)
        {
            await File.WriteAllBytesAsync(path, data);
            return true;
        }
        return false;
    }

    public async Task<Backup[]?> GetBackupAsync()
    {
        var request = new RestRequest(Globals.BACKUPS, Method.Get);
        var response = await _client.ExecuteAsync<Backup[]>(request).ConfigureAwait(false);
        Log.Information("[{Method}] {StatusDescription} ({StatusCode}): {URI}", 
            request.Method, response.StatusDescription, (int)response.StatusCode, request.Resource);

        return response.Data;
    }
}