using InsanityLib.Auto.Config.ConfigLib;
using InsanityLib.Interfaces;
using Newtonsoft.Json;
using ProperVersion;
using System.ComponentModel;

namespace InsanityLib.Util.Versioning;

public class Versioned<T> : IVersionedWrapper where T : class, new()
{
    [ReadOnly(true)]
    public SemVer Version { get; set; } = new SemVer(1, 0, 0);

    [ConfigDisplay(Hierarchy = EHierarchyDisplay.None)]
    public T Content { get; set; } = new();

    [Browsable(false)]
    [JsonIgnore]
    public object ContentAsObj => Content;

    public bool ReplaceIfNewer(Versioned<T> other)
    {
        if (other.Version > Version)
        {
            Version = other.Version;
            Content = other.Content;
            return true;
        }
        return false;
    }
}

public interface IVersionedWrapper : IVersioned
{
    object ContentAsObj { get; }
}