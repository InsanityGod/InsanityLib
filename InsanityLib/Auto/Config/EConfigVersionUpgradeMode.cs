namespace InsanityLib.Auto.Config;

public enum EConfigVersionUpgradeMode
{
    /// <summary>
    /// Creates a new instance of the config and then merges the old instance into the new one. <br />
    /// </summary>
    MergeIntoNew,

    /// <summary>
    /// Throw an exception if the version is not valid.
    /// </summary>
    Throw,

    /// <summary>
    /// Log a warning if the version is not valid.
    /// </summary>
    Warning
}