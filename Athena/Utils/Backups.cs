using System.Diagnostics;
using K4os.Compression.LZ4.Streams;
using CommunityToolkit.HighPerformance;
using CUE4Parse.UE4.Readers;

namespace Athena.Utils;

public static class FBackup
{
    private const uint _LZ4Magic = 0x184D2204u;
    private const uint _backupMagic = 0x504B4246;

    public static async Task<HashSet<string>> ParseBackup(FileInfo backupPath)
    {
        var entries = new HashSet<string>();
        
        var start = Stopwatch.StartNew();
        await using var memoryStream = new MemoryStream();
        await using var backupStream = new FileStream(backupPath.FullName, FileMode.Open);

        if (backupStream.Read<uint>() == _LZ4Magic)
        {
            backupStream.Position -= 4;
            using var compressionStream = LZ4Stream.Decode(backupStream);
            await compressionStream.CopyToAsync(memoryStream);
        }
        else await backupStream.CopyToAsync(memoryStream);

        backupStream.Position = 0;
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
            entries.Add(archive.ReadString().ToLower()[1..]);
        }

        Log.Information("Parsed backup {bkp} in {tot}s ({ms}ms). Version: {ver}.", 
            backupPath.Name, start.Elapsed.Seconds, start.ElapsedMilliseconds, version);

        return entries;
    }
}