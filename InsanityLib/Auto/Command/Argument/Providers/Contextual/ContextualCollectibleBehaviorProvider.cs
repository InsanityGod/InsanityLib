using InsanityLib.Extensions;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
public sealed class ContextualCollectibleBehaviorProvider : IContextualArgumentProvider<CollectibleBehavior?>
{
    private ContextualCollectibleBehaviorProvider() { }
    
    public static readonly ContextualCollectibleBehaviorProvider Instance = new();

    public bool CanProvide(IArgumentInfo argumentInfo, CommandParameterAttribute? attr) => typeof(CollectibleBehavior).IsAssignableFrom(argumentInfo.Type);

    public EContextualSource DefaultSource(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => typeof(BlockBehavior).IsAssignableFrom(argumentInfo.Type) ? EContextualSource.CallerTarget : EContextualSource.Caller;

    public CollectibleBehavior? Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, EContextualSource contextualSource) => (contextualSource switch
    {
        EContextualSource.Caller => ContextualCollectibleProvider.ProvideRaw(serviceProvider, argumentInfo, contextualSource, EnumItemClass.Item)?.GetBehavior(argumentInfo.Type),
        EContextualSource.CallerTarget => ContextualCollectibleProvider.ProvideRaw(serviceProvider, argumentInfo, contextualSource, EnumItemClass.Block)?.GetBehavior(argumentInfo.Type),
        _ => throw new NotSupportedException($"Unknown/Unsupported {nameof(EContextualSource)}: {contextualSource}"),
    })?.As<CollectibleBehavior>(argumentInfo.Type);
}
