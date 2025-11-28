namespace Athena.Models;

[AttributeUsage(AttributeTargets.Enum)]
public class DisabledFor(params EModelType[] disabledModels) : Attribute
{
    public EModelType[] DisabledModels = disabledModels;
}