using RestSharp;

namespace Athena.Rest;

public static class Endpoints
{
    private static readonly RestClient _client = new(new RestClientOptions()
    {
        UserAgent = $"Athena/{Globals.VERSION}"
    });

    public static readonly FNCentralEndpoints FNCentral = new(_client);
    public static readonly EpicGamesEnpoints Epic = new(_client);
    public static readonly AthenaEndpoints AthenaEndpoints = new(_client);
}