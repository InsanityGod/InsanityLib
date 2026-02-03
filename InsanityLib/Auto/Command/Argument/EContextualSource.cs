namespace InsanityLib.Auto.Command.Argument;

/// <summary>
/// Describes how/where <see cref="EParamProvider.ContextualProvider"/> should get it's information from.
/// </summary>
public enum EContextualSource
{
    None,

    /// <summary>
    /// Provided from the one calling it
    /// </summary>
    Caller = 1,
    
    /// <summary>
    /// Provided from what the one calling it is targeting (looking at)
    /// </summary>
    CallerTarget = 2,
}
