/* 
    Solitude ProfileBuilder.cs 
    File at https://github.com/OutTheShade/Solitude/blob/master/Solitude/Objects/Profile/ProfileBuilder.cs
*/

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Athena.Models;

namespace Athena.Managers;

public class ProfileBuilder
{
    private List<ProfileCosmetic> _cosmetics;
    private ProfileAthenaModel _profile;

    public ProfileBuilder(int reserve = 0)
    {
        _cosmetics = new(reserve);
        _profile = new();
    }

    public override string ToString() => Build();

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
            items[cosmetic.templateId] = JObject.FromObject(cosmetic);
        }

        return profileJson.ToString(Formatting.Indented);
    }

    public ProfileCosmetic OnCosmeticAdded(string id, Dictionary<string, List<string>> variants)
    {
        ProfileCosmetic ret;
        var prefix = id.Remove(id.IndexOf('_')).ToLower();

        switch (prefix)
        {
            case "cid":
            case "character":
                ret = new ProfileCosmetic(id, "AthenaCharacter", variants: variants);
                break;
            case "bid":
            case "backpack":
                ret = new ProfileCosmetic(id, "AthenaBackpack", variants: variants);
                break;
            case "pickaxe":
                ret = new ProfileCosmetic(id, "AthenaPickaxe", variants: variants);
                break;
            case "eid":
                ret = new ProfileCosmetic(id, "AthenaDance");
                break;
            case "glider":
                ret = new ProfileCosmetic(id, "AthenaGlider", variants: variants);
                break;
            case "wrap":
                ret = new ProfileCosmetic(id, "AthenaItemWrap");
                break;
            case "musicpack":
                ret = new ProfileCosmetic(id, "AthenaMusicPack");
                break;
            case "loadingscreen":
            case "lsid":
                ret = new ProfileCosmetic(id, "AthenaLoadingScreen");
                break;
            case "umbrella":
                ret = new ProfileCosmetic(id, "AthenaGlider");
                break;
            case "contrail":
            case "trails":
                ret = new ProfileCosmetic(id, "AthenaSkyDiveContrail");
                break;
            case "petcarrier":
                ret = new ProfileCosmetic(id, "AthenaBackpack", variants: variants);
                break;
            case "spray":
            case "spid":
            case "toy":
            case "emoji":
                ret = new ProfileCosmetic(id, "AthenaDance");
                break;
            default:
                ret = new ProfileCosmetic(id, "AthenaCharacter");
                break;
        };

        _cosmetics.Add(ret);
        return ret;
    }
}
