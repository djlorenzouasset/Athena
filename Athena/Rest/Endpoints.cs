using RestSharp;
using Athena.Models;

namespace Athena.Rest;

public static class Endpoints
{
    private static readonly RestClient Client = new();

    public static readonly FNCentralEndpoints FNCentral = new(Client);

    public static readonly EpicGamesEnpoints Epic = new(Client);

    public static async Task<bool> DownloadFileAsync(string url, string path)
    {
        var request = new RestRequest(url);
        var data = await Client.DownloadDataAsync(request);
        if (data is not null)
        {
            await File.WriteAllBytesAsync(path, data);
            return true;
        }
        return false;
    }

    public static async Task<Backup[]?> GetBackupAsync()
    {
        var request = new RestRequest(Globals.BACKUPS, Method.Get);
        var response = await Client.ExecuteAsync<Backup[]>(request).ConfigureAwait(false);
        Log.Information("[{Method}] {StatusDescription} ({StatusCode}): {URI}", request.Method, response.StatusDescription, (int)response.StatusCode, request.Resource);

        if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(response.Content))
        {
            Log.Error("Can't get the backup. Error while requesting the endpoint.");
            Console.ReadKey();
            Environment.Exit(0);
        }
        return response.Data;
    }
}