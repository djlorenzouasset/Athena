using RestSharp;

namespace Athena.Extensions;

public static class RestSharpExtensions
{
    public static void AddParameters(this RestRequest request, params Parameter[] parameters)
    {
        foreach (var param in parameters)
        {
            request.AddParameter(param);
        }
    }
}