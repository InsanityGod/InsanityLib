using System.Diagnostics;

namespace InsanityLib.Generators.Attributes;

/// <param name="path">Path to the config file. (<see cref="Path"/>)</param>
[Conditional("CompileTimeOnly")]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class AutoConfigAttribute(string path) : Attribute
{
    /// <summary>
    /// The path to the config file. <br />
    /// (relative to the config folder)
    /// </summary>
    public string Path { get; } = path;

    /// <summary>
    /// Whether the config file should be synced from server to client
    /// </summary>
    public bool ServerSync { get; set; }
}
