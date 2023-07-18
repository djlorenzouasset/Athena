﻿// Solitude ProfileBuilder.cs file at https://github.com/OutTheShade/Solitude/blob/master/Solitude/Objects/Profile/ProfileBuilder.cs

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
            items[cosmetic.TemplateId] = JObject.FromObject(cosmetic);
        }

        return profileJson.ToString(Formatting.Indented);
    }

    public ProfileCosmetic OnCosmeticAdded(string id)
    {
        ProfileCosmetic ret;
        var prefix = id.Remove(id.IndexOf('_')).ToLower();

        switch (prefix)
        {
            case "cid":
            case "character":
                ret = new ProfileCosmetic(id, "AthenaCharacter");
                _profile.items.sandbox_loadout.AddCharacter(ret.TemplateId);
                break;
            case "bid":
            case "backpack":
                ret = new ProfileCosmetic(id, "AthenaBackpack");
                _profile.items.sandbox_loadout.AddBackpack(ret.TemplateId);
                break;
            case "pickaxe":
                ret = new ProfileCosmetic(id, "AthenaPickaxe");
                _profile.items.sandbox_loadout.AddPickaxe(ret.TemplateId);
                break;
            case "eid":
                ret = new ProfileCosmetic(id, "AthenaDance");
                _profile.items.sandbox_loadout.AddDance(ret.TemplateId);
                break;
            case "glider":
                ret = new ProfileCosmetic(id, "AthenaGlider");
                _profile.items.sandbox_loadout.AddGlider(ret.TemplateId);
                break;
            case "wrap":
                ret = new ProfileCosmetic(id, "AthenaItemWrap");
                _profile.items.sandbox_loadout.AddItemWrap(ret.TemplateId);
                break;
            case "musicpack":
                ret = new ProfileCosmetic(id, "AthenaMusicPack");
                _profile.items.sandbox_loadout.AddMusicPack(ret.TemplateId);
                break;
            case "loadingscreen":
            case "lsid":
                ret = new ProfileCosmetic(id, "AthenaLoadingScreen");
                _profile.items.sandbox_loadout.AddLoadingScreen(ret.TemplateId);
                break;
            case "umbrella":
                ret = new ProfileCosmetic(id, "AthenaGlider");
                _profile.items.sandbox_loadout.AddGlider(ret.TemplateId);
                break;
            case "contrail":
            case "trails":
                ret = new ProfileCosmetic(id, "AthenaSkyDiveContrail");
                _profile.items.sandbox_loadout.AddContrail(ret.TemplateId);
                break;
            case "petcarrier":
                ret = new ProfileCosmetic(id, "AthenaBackpack");
                _profile.items.sandbox_loadout.AddBackpack(ret.TemplateId);
                break;
            case "spray":
            case "spid":
            case "toy":
            case "emoji":
                ret = new ProfileCosmetic(id, "AthenaDance");
                _profile.items.sandbox_loadout.AddDance(ret.TemplateId);
                break;
            default:
                ret = new ProfileCosmetic(id, "AthenaCharacter");
                break;
        };

        _cosmetics.Add(ret);
        return ret;
    }
}
