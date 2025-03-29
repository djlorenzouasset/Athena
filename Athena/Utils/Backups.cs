using System.Diagnostics;
using K4os.Compression.LZ4.Streams;
using CommunityToolkit.HighPerformance;
using CUE4Parse.UE4.Readers;
using Athena.Services;

namespace Athena.Utils;

public static class FBackup
{
    private const uint _LZ4Magic = 0x184D2204u;
    private const uint _backupMagic = 0x504B4246;
    
    public static async Task<FileInfo?> Download()
    {
        var backups = await APIEndpoints.Backups.GetBackupsAsync();
        if (backups is null || backups.Length == 0)
        {
            return null;
        }

        var backup = backups.Last();
        var file = new FileInfo(Path.Combine(Directories.Backups.FullName, backup.FileName));

        if (await APIEndpoints.DownloadFileAsync(backup.DownloadUrl, file.FullName))
        {
            return file; // directly return the previous instance of FileInfo
        }

        return null;
    }

    public static async Task<HashSet<string>> Parse(FileInfo backupPath)
    {
        var entries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        await using var memoryStream = new MemoryStream();
        await using var backupStream = new FileStream(backupPath.FullName, FileMode.Open);

        var start = Stopwatch.StartNew();

        if (backupStream.Read<uint>() == _LZ4Magic)
        {
            backupStream.Position -= 4;
            using var compressionStream = LZ4Stream.Decode(backupStream);
            await compressionStream.CopyToAsync(memoryStream);
        }
        else await backupStream.CopyToAsync(memoryStream);

        memoryStream.Position = 0;
        await using var archive = new FStreamArchive(backupStream.Name, memoryStream);

        if (archive.Read<uint>() != _backupMagic)
        {
            Log.Error("Backup has invalid magic.");
            return [];
        }

        var version = archive.Read<EBackupVersion>();
        var count = archive.Read<int>();
        for (var i = 0; i < count; i++)
        {
            archive.Position += sizeof(long) + sizeof(byte);
            var fullPath = archive.ReadString();
            if (version < EBackupVersion.PerfectPath) fullPath = fullPath[1..];
            entries.Add(fullPath);
        }

        Log.Information("Parsed backup {bkp} in {tot}s ({ms}ms). Version: {ver}.", backupPath.Name, 
            Math.Round(start.Elapsed.TotalSeconds, 2), Math.Round(start.Elapsed.TotalMilliseconds), version);

        return entries;
    }
}