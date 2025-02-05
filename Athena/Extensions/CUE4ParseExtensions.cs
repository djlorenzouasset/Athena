using CUE4Parse.UE4.VirtualFileSystem;

namespace Athena.Extensions;

public static class CUE4ParseExtensions
{
    public static string CreateObjectPath(this VfsEntry _entry)
        => $"{_entry.PathWithoutExtension}.{_entry.NameWithoutExtension}";
}