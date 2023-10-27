namespace Athena.Models;

public class AesKey
{
    public string MainKey { get; set; }
    public List<DynamicKey> DynamicKeys { get; set; }
}

public class DynamicKey
{
    public string Name { get; set; }
    public string Key { get; set; }
    public string Guid { get; set; }
    public Size Size { get; set; }
}

public class Size
{
    public string Formatted { get; set; }
}