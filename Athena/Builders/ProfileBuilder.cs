using Athena.Models.Profiles;

namespace Athena.Builders;

public class ProfileBuilder : BaseBuilder
{
    private readonly ProfileAthena _profile = new();
    private readonly List<Cosmetic> _cosmetics = [];

    public override string Build()
    {
        foreach (var cosmetic in _cosmetics)
        {
            // use a UUID or the game doesn't load companions variants for some reason
            _profile.Items.Add(Guid.NewGuid().ToString(), cosmetic);
        }

        return Serialize(_profile);
    }

    public void AddCosmetic(string id, string backendType, List<Variant> variants)
    {
        _cosmetics.Add(new(id, backendType, variants));
    }
}