using System;

namespace InsanityLib.Auto.Command.Argument;

/// <summary>
/// Marks a parameter to be used as a command argument.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class CommandParameterAttribute : Attribute
{
    private EParamProvider provider = EParamProvider.ArgumentParser;

    /// <summary>
    /// What provider should be used to provide the parameter.
    /// </summary>
    public EParamProvider Provider 
    { 
        get => ContextualSource == EContextualSource.None ? provider : EParamProvider.ContextualProvider;
        set => provider = value; 
    }

    /// <summary>
    /// The source from which the parameter should be provided.<br/>
    /// If set <see cref="Provider"/> will always be <see cref="EParamProvider.ContextualProvider"/>, if unset the default will be decided by the Contextual Provider.
    /// </summary>
    public EContextualSource ContextualSource { get; set; } = EContextualSource.None;

}
