namespace Athena.Models;

[AttributeUsage(AttributeTargets.Field)]
public class DisabledFor(params EModelType[] disabledModels) : Attribute
{
    public EModelType[] DisabledModels = disabledModels;
}