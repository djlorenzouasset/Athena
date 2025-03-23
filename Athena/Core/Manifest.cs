using EpicManifestParser;
using EpicManifestParser.UE;
using EpicManifestParser.Api;
using EpicManifestParser.ZlibngDotNetDecompressor;
using CUE4Parse.Compression;
using CUE4Parse.UE4.Readers;
using Athena.Services;

namespace Athena.Core;

public class ManifestDownloader
{
    public FBuildPatchAppManifest Manifest = null!;

    public string GameVersion = string.Empty;
    public string GameBuild = string.Empty;
    public string ManifestId = string.Empty;
    public string ManifestFileName => ManifestId + ".manifest";

    private const string CHUNKS_ENDPOINT = "https://epicgames-download1.akamaized.net/Builds/Fortnite/CloudDir/";

    public async Task DownloadManifest(ManifestInfo manifest)
    {
        var options = new ManifestParseOptions
        {
            ChunkBaseUrl = CHUNKS_ENDPOINT,
            Decompressor = ManifestZlibngDotNetDecompressor.Decompress,
            DecompressorState = ZlibHelper.Instance,
            ChunkCacheDirectory = Directories.Data.FullName,
            ManifestCacheDirectory = Directories.Data.FullName,
        };

        InitInformations(manifest); // save some informations that we need later
        (Manifest, _) = await manifest.DownloadAndParseAsync(options,
            elementManifestPredicate: static x => x.Uri.Host != "download.epicgames.com");
    }

    private void InitInformations(ManifestInfo manifest)
    {
        var uri = manifest.Elements[0].Manifests[0].Uri;

        var parsed = manifest.Elements[0].TryParseVersionAndCL(out var ver, out _);
        if (!parsed || ver is null) // this is very unlikely
            return;

        GameBuild = Manifest.Meta.BuildVersion;
        GameVersion = ver.ToString(2);
        ManifestId = uri.AbsolutePath.Split('/').Last();
    }

    public void LoadManifestArchives()
    { }
}