using System.Net;
using RestSharp;
using Athena.Rest;
using Athena.Models;

public class EpicGamesEnpoints : RestBase
{
    public EpicGamesEnpoints(RestClient client) : base(client) 
    {
    }

    public async Task<AuthResponse?> CreateAuthAsync()
    {
        var request = new RestRequest(Globals.AUTH, Method.Post);
        request.AddHeader("Authorization", Globals.BASIC);
        request.AddParameter("grant_type", "client_credentials");
        var response = await _client.ExecuteAsync<AuthResponse>(request).ConfigureAwait(false);
        Log.Information("[{Method}] {StatusDescription} ({StatusCode}): {URI}", request.Method, response.StatusDescription, (int)response.StatusCode, request.Resource);

        if (response is null || !response.IsSuccessful || string.IsNullOrEmpty(response.Content)) return null;
        return response.Data;
    }

    public async Task<bool> IsAuthValid()
    {
        var request = new RestRequest(Globals.VERIFY, Method.Get);
        request.AddHeader("Authorization", "Bearer " + Config.config.accessToken);
        var response = await _client.ExecuteAsync(request).ConfigureAwait(false);
        Log.Information("[{Method}] {StatusDescription} ({StatusCode}): {URI}", request.Method, response.StatusDescription, (int)response.StatusCode, request.Resource);
        return response.StatusCode == HttpStatusCode.OK;
    }

    public async Task<RestResponse?> GetManifestAsync()
    {
        var request = new RestRequest(Globals.MANIFEST, Method.Get);
        request.AddHeader("Authorization", "Bearer " + Config.config.accessToken);
        var response = await _client.ExecuteAsync(request).ConfigureAwait(false);
        Log.Information("[{Method}] {StatusDescription} ({StatusCode}): {URI}", request.Method, response.StatusDescription, (int)response.StatusCode, request.Resource);

        if (response is null || !response.IsSuccessful || string.IsNullOrEmpty(response.Content)) return null;
        return response;
    }
}