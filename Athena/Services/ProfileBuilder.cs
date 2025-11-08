using Athena.Models.App;

namespace Athena.Services;

public class ProfileBuilder : BaseBuilder
{
    private readonly ProfileAthena _profile = new();
    private readonly List<Cosmetic> _cosmetics = [];

    public override string Build()
    {
        foreach (var cosmetic in _cosmetics)
        {
            _profile.Items.Add(Guid.NewGuid().ToString(), cosmetic);
        }

        return Serialize(_profile);
    }

    public void AddCosmetic(string id, string backendType, List<ProfileAthena.Variant> variants)
    {
        _cosmetics.Add(new(id, backendType, variants));
    }
}