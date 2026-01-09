using InsanityLib.Extensions;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
#nullable enable
public sealed class ContextualBlockSelectionProvider : IContextualArgumentProvider<BlockSelection?>
{
    private ContextualBlockSelectionProvider() { }
    
    public static readonly ContextualBlockSelectionProvider Instance = new();

    public EContextualSource DefaultSource(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => EContextualSource.Caller;

    public BlockSelection? Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, EContextualSource contextualSource) => contextualSource switch
    {
        EContextualSource.Caller or EContextualSource.CallerTarget => serviceProvider.GetService<Caller>().Player.Required(contextualSource, parameterInfo.ParameterType).CurrentBlockSelection,
        _ => throw new NotSupportedException($"Unknown/Unsupported {nameof(EContextualSource)}: {contextualSource}"),
    };
}
