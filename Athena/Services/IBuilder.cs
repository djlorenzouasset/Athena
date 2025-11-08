using Newtonsoft.Json;
using Athena.Models.App;

namespace Athena.Services;

public interface IBuilder
{
    string Build();
}

public abstract class BaseBuilder : IBuilder
{
    public abstract string Build();

    protected static string Serialize(object obj) =>
        JsonConvert.SerializeObject(obj, Formatting.Indented, Globals.JsonSettings);
}