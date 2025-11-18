namespace Athena.Models.Settings;

public class ProfileSettings
{
    public int BattlePassLevel { get; set; } = -1;
    public string ProfileId { get; set; } = "AthenaProfile";
    public string OutputPath { get; set; } = Directories.Output;
}