/* 
   OutTheShade - Solitude: ProfileBuilder.cs (modified by me for latest)
*/

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Athena.Models;

namespace Athena.Managers;

public class ProfileBuilder
{
    private readonly int sparksLastIndex = 7;
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
        var prefix = id.Contains("Sparks_") 
            ? id.Remove(id.IndexOf('_', sparksLastIndex)).ToLower() 
            : id.Remove(id.IndexOf('_')).ToLower();

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
                ret = new ProfileCosmetic(id, "AthenaDance", variants);
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
                ret = new ProfileCosmetic(id, "AthenaGlider", variants);
                break;
            case "contrail":
            case "trails":
                ret = new ProfileCosmetic(id, "AthenaSkyDiveContrail", variants);
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

            /* THE FIX BELLOW IS NOT DEFINITIVE */

            // vehicles
            case "id":
                var type = id.Split('_')[1];
                ret = new ProfileCosmetic(id, $"VehicleCosmetics_{type}", variants);
                break;

            // instruments
            case "sparks_mic":
                ret = new ProfileCosmetic(id, "SparksMicrophone", variants);
                break;
            case "sparks_keytar":
                ret = new ProfileCosmetic(id, "SparksKeyboard", variants);
                break;
            case "sparks_guitar":
                ret = new ProfileCosmetic(id, "SparksGuitar", variants);
                break;
            case "sparks_drum":
                ret = new ProfileCosmetic(id, "SparksDrums", variants);
                break;
            case "sparks_bass":
                ret = new ProfileCosmetic(id, "SparksBass", variants);
                break;
            case "sparksaura":
                ret = new ProfileCosmetic(id, "SparksAura", variants);
                break;
            case "sid":
                ret = new ProfileCosmetic(id, "SparksSong", variants);
                break;

            // other
            default:
                ret = new ProfileCosmetic(id, "TBD", variants);
                break;
        };

        _cosmetics.Add(ret);
        return ret;
    }
}
