using ProperVersion;

namespace InsanityLib.Interfaces;

public interface IVersioned
{
    SemVer Version { get; set; }
}
