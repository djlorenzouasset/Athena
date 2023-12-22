using Newtonsoft.Json;
using Athena.Models;

namespace Athena.Managers;

public class CommonCoreBuilder
{
    private ProfileCommonCore _profile;

    public CommonCoreBuilder()
    {
        _profile = new();
    }

    public string Build() => JsonConvert.SerializeObject(_profile, Formatting.Indented);

    public void SetVbucksAmount(int amount = 0)
    {
        _profile.items.currencyAttributes.quantity = amount;
    }

    // not used atm, here just for a future use (i dont know lol)
    public void SetCreatorCode(string creatorCode) 
    {
        _profile.stats.attributes.mtx_affiliate = creatorCode;
    }
}