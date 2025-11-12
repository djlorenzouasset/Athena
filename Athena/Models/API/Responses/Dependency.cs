namespace Athena.Models.API.Responses;

public class Dependency
{
    public string Filename;
    public bool Required;
    public string DownloadUrl;

    public async Task<bool> Download(string dest)
    {
        var file = await Api.DownloadFileAsync(DownloadUrl, dest);
        return file.Exists;
    }
}