namespace Athena.Models.Settings;

public class ChunksSettings
{
    public bool AutoClearEnabled { get; set; } = true;
    public int ChunkCacheLifetime { get; set; } = 7;
}