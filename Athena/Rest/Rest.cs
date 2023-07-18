using RestSharp;

namespace Athena.Rest;

public abstract class RestBase
{
    protected readonly RestClient Client;

    protected RestBase(RestClient client)
    {
        Client = client;
    }
}