using CUE4Parse.UE4.Objects.Core.Misc;
using Newtonsoft.Json;

namespace Athena.Models.API.Responses;

public class AESKeys
{
    public string MainKey;
    public List<DynamicKey> DynamicKeys;

    [JsonIgnore] public IReadOnlyList<FGuid> GuidsList => [..DynamicKeys.Select(x => new FGuid(x.Guid))];
}

public class DynamicKey
{
    public string Name;
    public string Key;
    public string Guid;
    public int FileCount;
    public Size Size;
}

public class Size
{
    public string Formatted;
}