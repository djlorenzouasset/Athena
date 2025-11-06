using RestSharp;

namespace Athena.Models.API.Base;

public abstract class AthenaRestClient(RestClient client)
{
    protected virtual string BaseURL => string.Empty;

    /* client used in every AthenaRestClient inherited class */
    protected readonly RestClient _client = client;

    protected async Task<T?> ExecuteAsync<T>(string url, Method method = Method.Get, bool bLog = true, params Parameter[] prms)
    {
        string finalUrl = string.IsNullOrEmpty(BaseURL) ? url : $"{BaseURL}/{url}";

        var request = new RestRequest(finalUrl, method);
        foreach (var param in prms) request.AddParameter(param);

        var response = await _client.ExecuteAsync<T>(request).ConfigureAwait(false);
        if (bLog)
        {
            Log.Information("[{Method}] {StatusDescription} ({StatusCode}): {Uri}", request.Method,
                response.StatusDescription, (int)response.StatusCode, request.Resource);
        }

        return response.Data;
    }
    
    protected async Task<RestResponse> ExecuteAsync(string url, Method method = Method.Get, bool bLog = true, params Parameter[] prms)
    {
        string finalUrl = string.IsNullOrEmpty(BaseURL) ? url : $"{BaseURL}/{url}";

        var request = new RestRequest(finalUrl, method);
        foreach (var param in prms) request.AddParameter(param);

        var response = await _client.ExecuteAsync(request).ConfigureAwait(false);
        if (bLog)
        {
            Log.Information("[{Method}] {StatusDescription} ({StatusCode}): {Uri}", request.Method,
                response.StatusDescription, (int)response.StatusCode, request.Resource);
        }

        return response;
    }
}