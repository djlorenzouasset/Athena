using System.Text.RegularExpressions;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Readers;
using EpicManifestParser.Objects;

namespace Athena.Managers;

public class ManifestDownloader
{
    public string _endpoint { get; init; }
    public Manifest? ManifestFile { get; private set; }

    // regex from FModel
    public static Regex PaksFinder = new(@"^FortniteGame(/|\\)Content(/|\\)Paks(/|\\)(pakchunk(?:0|10.*|\w+)-WindowsClient|global)\.(pak|utoc)$",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public ManifestDownloader(string endpoint)
    {
        _endpoint = endpoint;
    }

    public async Task DownloadManifest(ManifestInfo manifestInfo)
    {
        ManifestFile = new(await manifestInfo.DownloadManifestDataAsync(), new()
        {
            ChunkBaseUri = new(_endpoint, UriKind.Absolute),
            ChunkCacheDirectory = new(DirectoryManager.ChunksDir)
        });
    }

    public void LoadFileManifest(FileManifest file, ref StreamedFileProvider _provider)
    {
        var versions = _provider.Versions;

        if (!PaksFinder.IsMatch(file.Name)) return;

        if (file.Name.EndsWith(".utoc"))
        {
            _provider.RegisterVfs(
                file.Name, new Stream[] { file.GetStream() }, it => new FStreamArchive(it, ManifestFile.FileManifests.First(x => x.Name.Equals(it)).GetStream(), versions)
            );
        }
        else
        {
            var stream = file.GetStream();
            _provider.RegisterVfs(file.Name, new[] { stream });
        }
    }
}