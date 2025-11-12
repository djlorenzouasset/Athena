namespace Athena.Models;

public class DisabledFor(params EModelType[] disabledModels) : Attribute
{
    public EModelType[] DisabledModels = disabledModels;
}