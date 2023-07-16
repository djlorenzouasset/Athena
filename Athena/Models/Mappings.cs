namespace Athena.Models;

public class Mappings
{
    public string Url { get; set; }
    public string Filename { get; set; }

    public bool IsValid => Url is not null && !string.IsNullOrEmpty(Url) && !string.IsNullOrEmpty(Filename);
}