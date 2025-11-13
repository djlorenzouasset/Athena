using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Athena.Utils;

public static class CustomJsonSerializer
{
    private static readonly JsonSerializerSettings _serializerSettings = new()
    {
        Formatting = Formatting.Indented,
        ObjectCreationHandling = ObjectCreationHandling.Replace,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
    };

    public static string Serialize(object obj) => JsonConvert.SerializeObject(obj, _serializerSettings);
    public static T? Deserialize<T>(string str) => JsonConvert.DeserializeObject<T>(str, _serializerSettings);
}