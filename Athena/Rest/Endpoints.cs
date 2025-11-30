using RestSharp;
using Athena.Rest.Endpoints;

namespace Athena.Rest;

public static class APIEndpoints
{
    private static readonly RestClient _client = new(new RestClientOptions()
    {
        UserAgent = $"Athena/{Globals.VERSION}"
    });

    public static readonly DillyEndpoints Dilly = new(_client);
    public static readonly EpicGamesEnpoints Epic = new(_client);
    public static readonly AthenaEndpoints AthenaEndpoints = new(_client);
}