namespace Athena.Models.Settings;

public interface IOption
{
    string ViolatorTag { get; }
    CardOptions CardOptions { get; }
}