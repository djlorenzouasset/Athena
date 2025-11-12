using System.Text.RegularExpressions;
using EpicManifestParser;
using EpicManifestParser.UE;
using EpicManifestParser.Api;
using EpicManifestParser.ZlibngDotNetDecompressor;
using CUE4Parse.UE4.Readers;
using CUE4Parse.Compression;

namespace Athena.Services;

public partial class ManifestService
{
    public FBuildPatchAppManifest Manifest = null!;

    public string GameVersion = string.Empty;
    public string GameBuild = string.Empty;
    public string ManifestId = string.Empty;

    private const string CHUNKS_ENDPOINT = "http://download.epicgames.com/Builds/Fortnite/CloudDir/";

    public async Task DownloadManifest(ManifestInfo manifest)
    {
        var options = new ManifestParseOptions
        {
            ChunkBaseUrl = CHUNKS_ENDPOINT,
            Decompressor = ManifestZlibngDotNetDecompressor.Decompress,
            DecompressorState = ZlibHelper.Instance,
            ChunkCacheDirectory = Directories.Data,
            ManifestCacheDirectory = Directories.Data,
        };

        (Manifest, _) = await manifest.DownloadAndParseAsync(options,
            elementManifestPredicate: static x => x.Uri.Host == "download.epicgames.com");

        InitInformations(manifest); // save some informations that we need later
    }

    private void InitInformations(ManifestInfo manifest)
    {
        GameBuild = Manifest.Meta.BuildVersion;

        var data = GameBuild.Split('-');
        if (data.Length > 2)
        {
            GameVersion = data[1];
        }

        var uri = manifest.Elements[0].Manifests[0].Uri;
        ManifestId = uri.AbsolutePath.Split('/').Last();
    }

    public void LoadManifestArchives()
    {
        Manifest.Files
            .Where(x => MyRegex().IsMatch(x.FileName))
            .AsParallel()
            .WithDegreeOfParallelism(8)
            .ForAll(file => LoadFileManifest(file));
    }

    private void LoadFileManifest(FFileManifest file)
    {
        var versions = UEParser.Provider.Versions;
        UEParser.Provider.RegisterVfs(file.FileName, [file.GetStream()], 
            it => new FRandomAccessStreamArchive(it, GetStream(it), versions));
    }

    private FFileManifestStream GetStream(string fileName)
    {
        return Manifest.FindFile(fileName)!.GetStream();
    }

    [GeneratedRegex(@"^FortniteGame[/\\]Content[/\\]Paks[/\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex MyRegex();
}