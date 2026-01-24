using System;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Config;

public interface IAutoConfig<out T> : IAutoConfig where T : class, new()
{
    /// <summary>
    /// The loaded instance of the config
    /// </summary>
    new T? ConfigInstance { get; }

    object? IAutoConfig.ConfigInstance => ConfigInstance;

    Type IAutoConfig.ConfigInstanceType => typeof(T);
}

public interface IAutoConfig
{
    /// <summary>
    /// The path to the config file. <br />
    /// (relative to the config folder)
    /// </summary>
    public string RelativePath { get; }

    /// <summary>
    /// The loaded instance of the config
    /// </summary>
    object? ConfigInstance { get; }

    /// <summary>
    /// The type associated with the ConfigInstance
    /// </summary>
    Type ConfigInstanceType { get; }

    /// <summary>
    /// Whether the config file should be synced from server to client
    /// </summary>
    bool ServerSync { get; }

    /// <summary>
    /// Loads the config or creates it if non existing
    /// </summary>
    /// <returns>true if successful, false if an exception occured</returns>
    public bool TryLoadConfig(ICoreAPI api, ILogger logger);

    /// <summary>
    /// Attempts to save the config
    /// </summary>
    /// <returns>true if successful, false if an exception occured</returns>
    public bool TrySaveConfig(ICoreAPI api, ILogger logger);
}