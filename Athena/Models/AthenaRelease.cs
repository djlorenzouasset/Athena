namespace Athena.Models;

public class AthenaRelease
{
    public string Version { get; set; }
    public string Download { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Changelog { get; set; }
    public string UpdaterFile { get; set; }
    public List<string> Authors { get; set; }
    public List<string> Notices { get; set; }

    public bool VersionChanged() => !Globals.VERSION.Equals(Version);
}