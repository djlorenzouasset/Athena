using RestSharp;
using EpicManifestParser.Api;
using Athena.Models.API.Fortnite;

namespace Athena.Rest.Endpoints;

public class EpicGamesAPI(RestClient client) : AthenaRestClient(client)
{
    private const string OAUTH_ENDPOINT = "https://account-public-service-prod03.ol.epicgames.com/account/api/oauth/token";
    private const string MANIFEST_ENDPOINT = "https://launcher-public-service-prod06.ol.epicgames.com/launcher/api/public/assets/v2/platform/Windows/namespace/fn/catalogItem/4fe75bbc5a674f4f9b356b5c90567da5/app/Fortnite/label/Live";
    private const string BASIC_TOKEN = "basic M2Y2OWU1NmM3NjQ5NDkyYzhjYzI5ZjFhZjA4YThhMTI6YjUxZWU5Y2IxMjIzNGY1MGE2OWVmYTY3ZWY1MzgxMmU=";

    public async Task<EpicAuth?> CreateAuthAsync()
    {
        return await ExecuteAsync<EpicAuth?>(OAUTH_ENDPOINT, Method.Post, true,
            new HeaderParameter("Authorization", BASIC_TOKEN),
            new GetOrPostParameter("grant_type", "client_credentials"));
    }

    public async Task<bool> ValidateAuthAsync(EpicAuth auth)
    {
        var response = await ExecuteAsync(OAUTH_ENDPOINT, Method.Get, true,
            new HeaderParameter("Authorization", $"Bearer {auth.AccessToken}"));

        return response.IsSuccessful;
    }

    public async Task<ManifestInfo?> GetManifestAsync(EpicAuth auth, bool bLog = true)
    {
        var response = await ExecuteAsync(MANIFEST_ENDPOINT, Method.Get, bLog,
            new HeaderParameter("Authorization", $"bearer {auth.AccessToken}"));

        return response.IsSuccessful ? ManifestInfo.Deserialize(response.RawBytes) : null;
    }
}