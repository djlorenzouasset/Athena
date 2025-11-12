using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Athena.Utils;

public static class CustomJsonSerializer
{
    private static readonly JsonSerializerSettings _jsonSettings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy
            {
                OverrideSpecifiedNames = true
            }
        },
        Formatting = Formatting.Indented
    };

    public static string SerializeObject(object obj)
        => JsonConvert.SerializeObject(obj, _jsonSettings);
}