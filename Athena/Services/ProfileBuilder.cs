/* 
   OutTheShade - Solitude: ProfileBuilder.cs (modified)
*/

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Athena.Models;

namespace Athena.Managers;

public class ProfileBuilder
{
    private ProfileAthena _profile;
    private List<ProfileCosmetic> _cosmetics;

    public ProfileBuilder()
    {        
        _profile = new();
        _cosmetics = new();
    }

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

    public ProfileCosmetic AddCosmetic(string id, Dictionary<string, List<string>> variants)
    {
        ProfileCosmetic ret;
        var prefix = id.Remove(id.IndexOf('_')).ToLower();

        switch (prefix)
        {
            case "cid":
            case "character":
                ret = new ProfileCosmetic(id, "AthenaCharacter", variants);
                break;
            case "bid":
            case "backpack":
                ret = new ProfileCosmetic(id, "AthenaBackpack", variants);
                break;
            case "pickaxe":
                ret = new ProfileCosmetic(id, "AthenaPickaxe", variants);
                break;
            case "eid":
                ret = new ProfileCosmetic(id, "AthenaDance");
                break;
            case "glider":
                ret = new ProfileCosmetic(id, "AthenaGlider", variants);
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
                ret = new ProfileCosmetic(id, "AthenaBackpack", variants);
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
