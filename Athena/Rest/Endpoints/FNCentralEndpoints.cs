using RestSharp;
using Athena.Models;
using Athena.Managers;

namespace Athena.Rest.Endpoints;

public class FNCentralEndpoints(RestClient client) : RestBase(client)
{
    public Mappings[] Mappings = null!;
    public AesKey AesKey = null!;

    public async Task<string?> GetMappingsAsync()
    {
        // scuffed way but not final work
        string endpoint = "";
        if (Globals.bUseV2Endpoints)
            endpoint = "https://athena.djlorexzo.dev/api/v1/mappings";
        else
            endpoint = Globals.MAPPINGS;

        var request = new RestRequest(endpoint, Method.Get);
        var response = await _client.ExecuteAsync<Mappings[]>(request).ConfigureAwait(false);
        Log.Information("[{Method}] {StatusDescription} ({StatusCode}): {URI}", 
            request.Method, response.StatusDescription, (int)response.StatusCode, request.Resource);

        if (response == null || response.Data == null || response.Data.Length == 0) 
            return null;

        Mappings = response.Data;
        string path = Path.Combine(DirectoryManager.MappingsDir, Mappings[0].Filename);
        if (!await DownloadMappingsAsync(Mappings[0].Url, path)) return null;
        return path;
    }

    public async Task<AesKey?> GetAesKeysAsync()
    {
        var request = new RestRequest(Globals.AESKEYS, Method.Get);
        var response = await _client.ExecuteAsync<AesKey>(request).ConfigureAwait(false);
        Log.Information("[{Method}] {StatusDescription} ({StatusCode}): {URI}", 
            request.Method, response.StatusDescription, (int)response.StatusCode, request.Resource);

        if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(response.Content)) return null;
        AesKey = response.Data!;
        return response.Data;
    }

    public async Task<bool> DownloadMappingsAsync(string url, string mappingName)
    {
        var request = new RestRequest(url);
        var data = await _client.DownloadDataAsync(request);
        if (data is null) return false;

        await File.WriteAllBytesAsync(mappingName, data);
        return true;
    }
}