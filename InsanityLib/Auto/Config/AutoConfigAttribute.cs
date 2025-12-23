using InsanityLib.Util;
using System;

namespace InsanityLib.Auto.Config;

/// <param name="path">Path to the config file. (<see cref="Path"/>)</param>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class AutoConfigAttribute(string path) : Attribute
{
    /// <summary>
    /// The path to the config file. <br />
    /// (relative to the config folder)
    /// </summary>
    public string Path { get; init; } = path.EnsureFileExtension("json");

    /// <summary>
    /// Whether it should load default values if the config file encounters an error while loading. <br />
    /// </summary>
    public bool DefaultOnError { get; init; } = true;

    /// <summary>
    /// Whether the config should be automatically created if it does not exist yet.
    /// </summary>
    public bool CreateIfNotExist { get; init; } = true;

    /// <summary>
    /// Whether the config file should be synced from server to client
    /// </summary>
    public bool ServerSync { get; init; }
}
