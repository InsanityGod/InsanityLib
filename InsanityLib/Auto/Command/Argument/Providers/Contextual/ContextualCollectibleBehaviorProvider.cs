using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
public sealed class ContextualCollectibleBehaviorProvider : IContextualArgumentProvider<CollectibleBehavior?>
{
    private ContextualCollectibleBehaviorProvider() { }
    
    public static readonly ContextualCollectibleBehaviorProvider Instance = new();

    public bool CanProvide(ParameterInfo paramInfo, CommandParameterAttribute? attr) => typeof(CollectibleBehavior).IsAssignableFrom(paramInfo.ParameterType);

    public EContextualSource DefaultSource(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => typeof(BlockBehavior).IsAssignableFrom(parameterInfo.ParameterType) ? EContextualSource.CallerTarget : EContextualSource.Caller;

    public CollectibleBehavior? Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, EContextualSource contextualSource) => (contextualSource switch
    {
        EContextualSource.Caller => ContextualCollectibleProvider.ProvideRaw(serviceProvider, parameterInfo, contextualSource, EnumItemClass.Item)?.GetBehavior(parameterInfo.ParameterType),
        EContextualSource.CallerTarget => ContextualCollectibleProvider.ProvideRaw(serviceProvider, parameterInfo, contextualSource, EnumItemClass.Block)?.GetBehavior(parameterInfo.ParameterType),
        _ => throw new NotSupportedException($"Unknown/Unsupported {nameof(EContextualSource)}: {contextualSource}"),
    })?.As<CollectibleBehavior>(parameterInfo.ParameterType);
}
