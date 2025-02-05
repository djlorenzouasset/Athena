namespace Athena.Utils;

public static class FBackup
{
    private const uint _LZ4Magic = 0x184D2204u;
    private const uint _backupMagic = 0x504B4246;

    public static HashSet<string> ParseBackup(string backupPath)
    {
        var entries = new HashSet<string>();
        // @TODO: backups parsing logic

        return entries;
    }
}