using InsanityLib.Extensions;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
public sealed class ContextualCollectibleProvider : IContextualArgumentProvider<CollectibleObject?>
{
    private ContextualCollectibleProvider() { }
    
    public static readonly ContextualCollectibleProvider Instance = new();

    public EContextualSource DefaultSource(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => typeof(Block).IsAssignableFrom(argumentInfo.Type) ? EContextualSource.CallerTarget : EContextualSource.Caller;

    public bool CanProvide(IArgumentInfo argumentInfo, CommandParameterAttribute? attr) => typeof(CollectibleObject).IsAssignableFrom(argumentInfo.Type);

    public CollectibleObject? Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, EContextualSource contextualSource) =>
        ProvideRaw(serviceProvider, argumentInfo, contextualSource, typeof(Block).IsAssignableFrom(argumentInfo.Type) ? EnumItemClass.Block : EnumItemClass.Item)?.As<CollectibleObject>(argumentInfo.Type);

    public static CollectibleObject? ProvideRaw(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, EContextualSource contextualSource, EnumItemClass itemType)
    {
        if (itemType == EnumItemClass.Block) return contextualSource switch
        {
            EContextualSource.Caller => serviceProvider.GetService<IWorldAccessor>()!.BlockAccessor.GetBlock(serviceProvider.GetService<Caller>()!.Entity.Required(contextualSource, argumentInfo.Type).Pos.AsBlockPos.Down()),
            EContextualSource.CallerTarget => ContextualBlockSelectionProvider.Instance.Provide(serviceProvider, argumentInfo, contextualSource)?.GetOrFindBlock(serviceProvider.GetService<IWorldAccessor>()!),
            _ => throw new NotSupportedException($"Unknown/Unsupported {nameof(EContextualSource)}: {contextualSource}"),
        };

        return ContextualItemStackProvider.Instance.Provide(serviceProvider, argumentInfo, contextualSource)?.Collectible;
    }
}
