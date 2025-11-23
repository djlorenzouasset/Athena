using CUE4Parse.UE4.Objects.Core.Misc;
using Newtonsoft.Json;

namespace Athena.Models.API.Responses;

public class AESKeys
{
    public string MainKey;
    public List<DynamicKey> DynamicKeys;
}

public class DynamicKey
{
    public string Key;
    public string Guid;
}