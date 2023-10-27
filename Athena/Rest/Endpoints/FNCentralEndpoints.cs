using RestSharp;
using Athena.Rest;
using Athena.Models;
using Athena.Managers;

public class FNCentralEndpoints : RestBase
{
    public Mappings[]? Mappings;
    public AesKey? AesKey;

    public FNCentralEndpoints(RestClient client) : base(client)
    {
    }

    public async Task<Mappings[]?> GetMappingsAsync()
    {
        var request = new RestRequest(Globals.MAPPINGS, Method.Get);
        var response = await _client.ExecuteAsync<Mappings[]>(request).ConfigureAwait(false);
        Log.Information("[{Method}] {StatusDescription} ({StatusCode}): {URI}", request.Method, response.StatusDescription, (int)response.StatusCode, request.Resource);

        if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(response.Content)) return null;
        Mappings = response.Data;
        return response.Data;
    }

    public async Task<AesKey?> GetAesKeysAsync()
    {
        var request = new RestRequest(Globals.AESKEYS, Method.Get);
        var response = await _client.ExecuteAsync<AesKey>(request).ConfigureAwait(false);
        Log.Information("[{Method}] {StatusDescription} ({StatusCode}): {URI}", request.Method, response.StatusDescription, (int)response.StatusCode, request.Resource);

        if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(response.Content)) return null;
        AesKey = response.Data;
        return response.Data;
    }

    public async Task<bool> DownloadMappingsAsync(string url, string mappingName)
    {
        var request = new RestRequest(url);
        var data = await _client.DownloadDataAsync(request);
        if (data is not null) await File.WriteAllBytesAsync(Path.Combine(DirectoryManager.MappingsDir, mappingName), data);
        return true;
    }
}