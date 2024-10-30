using System.Diagnostics;
using System.Text.RegularExpressions;
using OffiUtils;
using EpicManifestParser;
using EpicManifestParser.UE;
using EpicManifestParser.Api;
using CUE4Parse.UE4.Readers;
using CUE4Parse.Compression;
using Athena.Managers;

namespace Athena.Services;

public class ManifestDownloader
{
    public FBuildPatchAppManifest Manifest = null!;

    public int CLVersion = 0;
    public string GameVersion = string.Empty;
    public string GameBuild = string.Empty;
    public string ManifestId = string.Empty;

    private const string _pattern = @"^FortniteGame(/|\\)Content(/|\\)Paks(/|\\)";
    private const string CHUNKS_ENDPOINT = "https://epicgames-download1.akamaized.net/Builds/Fortnite/CloudDir/";

    private readonly Dataminer _dataminer = Dataminer.Instance;
    private readonly Regex _pakFinder = new(_pattern, RegexOptions.Compiled |
        RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public async Task DownloadManifest(ManifestInfo manifest)
    {
        var options = new ManifestParseOptions
        {
            ChunkCacheDirectory = DirectoryManager.ChunksDir,
            ManifestCacheDirectory = DirectoryManager.ChunksDir,
            ChunkBaseUrl = CHUNKS_ENDPOINT,
            Zlibng = ZlibHelper.Instance
        };

        (Manifest, _) = await manifest.DownloadAndParseAsync(options);
        InitInformations(manifest);
    }

    private void InitInformations(ManifestInfo manifest)
    {
        GameBuild = Manifest.ManifestMeta.BuildVersion;

        var parsed = manifest.Elements[0].TryParseVersionAndCL(out var ver, out int cl);
        if (!parsed || ver is null)
            return;

        GameVersion = ver.ToString(2);
        CLVersion = cl;

        var uri = manifest.Elements[0].Manifests[0].Uri;
        ManifestId = uri.AbsolutePath.Split('/').Last();
    }

    public void LoadArchives()
    {
        foreach (var file in Manifest.FileManifestList)
        {
            if (file.FileName.Contains("optional"))
                continue;

            LoadFileManifest(file);
        }
    }

    private void LoadFileManifest(FFileManifest file)
    {
        var timer = Stopwatch.StartNew(); /* starts the stopwatch for calculate the loading time */
        var versions = _dataminer.Provider.Versions;

        if (!_pakFinder.IsMatch(file.FileName))
            return;

        else if (file.FileName.EndsWith(".ucas") || file.FileName.EndsWith(".sig"))
            return;

        else if (file.FileName.EndsWith(".utoc"))
        {
            var _stream = file.GetStream();
            _dataminer.Provider.RegisterVfs(file.FileName, [(IRandomAccessStream)_stream],
                it => new FRandomAccessStreamArchive(it, GetStream(it), versions));
        }
        else
        {
            var _stream = file.GetStream();
            _dataminer.Provider.RegisterVfs(file.FileName, [(IRandomAccessStream)_stream], null);
        }

        timer.Stop();
        Log.Information("Downloaded {name} in {tot}s ({ms}ms)", file.FileName, 
            timer.Elapsed.Seconds, Math.Round(timer.Elapsed.TotalMilliseconds));
    }

    private FFileManifestStream GetStream(string fileName)
    {
        var _file = Manifest.FileManifestList.First(x => x.FileName.Equals(fileName));
        return _file.GetStream();
    }
}