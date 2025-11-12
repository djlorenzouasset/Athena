using Athena.Utils;

namespace Athena.Builders;

public abstract class BaseBuilder : IBuilder
{
    public abstract string Build();

    protected static string Serialize(object obj) =>
        CustomJsonSerializer.SerializeObject(obj);
}