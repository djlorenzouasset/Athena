using RestSharp;

namespace Athena.Rest;

public abstract class RestBase(RestClient client)
{
    protected readonly RestClient _client = client;
}