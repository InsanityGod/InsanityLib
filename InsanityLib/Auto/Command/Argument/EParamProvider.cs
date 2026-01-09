namespace InsanityLib.Auto.Command.Argument;

/// <summary>
/// Describes what provider is used to provide a parameter's value.
/// </summary>
public enum EParamProvider
{
    /// <summary>
    /// Should be provided by the service provider
    /// </summary>
    ServiceProvider = 0,

    /// <summary>
    /// Should be provided by an argument parser
    /// </summary>
    ArgumentParser = 1,

    /// <summary>
    /// Provides contextual information based on <see cref="EContextualSource"/>
    /// </summary>
    ContextualProvider = 2,

    /// <summary>
    /// Should just always use the default value
    /// </summary>
    DefaultValue = 3,
}
