using Newtonsoft.Json;

namespace Athena.Models.API.Responses;

public class AppNews
{
    public List<string> StaticNews;
    public List<VersionedNews> VersionedNews;

    public List<string> GetNews()
    {
        List<string> news = [..StaticNews];
        foreach (var vm in VersionedNews)
        {
            if (vm.TargetVersion == Globals.Version)
            {
                news.AddRange(vm.NewsMessages);
            }
        }
        return news;
    }
}

public class VersionedNews
{
    [JsonConverter(typeof(AthenaVersionConverter))]
    public AthenaVersion TargetVersion;

    public List<string> NewsMessages;
}