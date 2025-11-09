/* 
   OutTheShade - Solitude: ProfileBuilder.cs (modified by me for latest)
*/

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Athena.Models;

namespace Athena.Services;

public class ProfileBuilder
{
    private readonly ProfileAthena _profile = new();
    private readonly List<ProfileCosmetic> _cosmetics = [];

    public string Build()
    {
        var profileJson = JObject.FromObject(_profile);
        if (profileJson is null)
            return string.Empty;

        var items = profileJson["items"];
        if (items is null) 
            return string.Empty;

        foreach (var cosmetic in _cosmetics)
        {
            items[Guid.NewGuid().ToString()] = JObject.FromObject(cosmetic);
        }

        return profileJson.ToString(Formatting.Indented);
    }

    public void AddCosmetic(string id, Dictionary<string, List<string>> variants)
    {
        string backendType = Helper.GetItemBackendType(id);
        _cosmetics.Add(new(id, backendType, variants));
    }
}
