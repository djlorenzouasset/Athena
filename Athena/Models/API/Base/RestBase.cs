using RestSharp;
using Athena.Extensions;

namespace Athena.Models.API.Base;

public abstract class AthenaRestClient(RestClient client)
{
    protected virtual string BaseURL => string.Empty;

    protected readonly RestClient _client = client;

    protected async Task<T?> ExecuteAsync<T>(string url, Method method = Method.Get, bool bLog = true, params Parameter[] prms)
    {
        string finalUrl = string.IsNullOrEmpty(BaseURL) ? url : $"{BaseURL}/{url}";

        try
        {
            var request = new RestRequest(finalUrl, method);
            request.AddParameters(prms);

            var response = await _client.ExecuteAsync<T>(request).ConfigureAwait(false);
            if (bLog) Log.Information("[{Method}] {StatusDescription} ({StatusCode}): {Uri}", 
                request.Method, response.StatusDescription, (int)response.StatusCode, request.Resource);
            return response.IsSuccessful ? response.Data : default;
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message, ex.StackTrace);
            return default;
        }
    }
    
    protected async Task<RestResponse> ExecuteAsync(string url, Method method = Method.Get, bool bLog = true, params Parameter[] prms)
    {
        string finalUrl = string.IsNullOrEmpty(BaseURL) ? url : $"{BaseURL}/{url}";

        var request = new RestRequest(finalUrl, method);
        request.AddParameters(prms);

        var response = await _client.ExecuteAsync(request).ConfigureAwait(false);
        if (bLog) Log.Information("[{Method}] {StatusDescription} ({StatusCode}): {Uri}",
            request.Method, response.StatusDescription, (int)response.StatusCode, request.Resource);

        if (response.ErrorException is not null) Log.Error(response.ErrorException.ToString());
        return response;
    }
}