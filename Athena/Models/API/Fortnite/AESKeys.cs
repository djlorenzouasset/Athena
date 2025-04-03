namespace Athena.Models.API.Fortnite;

public class AESKeys
{
    public string MainKey;
    public List<DynamicKey> DynamicKeys;
}

public class DynamicKey
{
    public string Name;
    public string Key;
    public string Guid;
    public int FileCount;
    public Size Size;
}

public class Size
{
    public string Formatted;
}