using RestSharp;

namespace Athena.Rest;

public abstract class RestBase
{
    protected readonly RestClient _client;

    protected RestBase(RestClient client)
    {
        _client = client;
    }
}