using CUE4Parse.FileProvider.Objects;
using CUE4Parse.UE4.VirtualFileSystem;

namespace Athena.Extensions;

public static class CUE4ParseExtensions
{
    public static string GetSize(this IAesVfsReader ar)
    {
        string[] units = { "KB", "MB", "GB" };
        double size = ar.Length / 1024d;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return size % 1 == 0
            ? $"{(int)size}{units[unitIndex]}"
            : $"{size:0.##}{units[unitIndex]}";
    }

    public static bool HasFilter(this IAesVfsReader ar, Func<GameFile, bool> filter)
    {
        return ar.Files.Values.Any(filter);
    }
}