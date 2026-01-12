using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
public sealed class ContextualCollectibleProvider : IContextualArgumentProvider<CollectibleObject?>
{
    private ContextualCollectibleProvider() { }
    
    public static readonly ContextualCollectibleProvider Instance = new();

    public EContextualSource DefaultSource(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => typeof(Block).IsAssignableFrom(parameterInfo.ParameterType) ? EContextualSource.CallerTarget : EContextualSource.Caller;

    public bool CanProvide(ParameterInfo paramInfo, CommandParameterAttribute? attr) => typeof(CollectibleObject).IsAssignableFrom(paramInfo.ParameterType);

    public CollectibleObject? Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, EContextualSource contextualSource) =>
        ProvideRaw(serviceProvider, parameterInfo, contextualSource, typeof(Block).IsAssignableFrom(parameterInfo.ParameterType) ? EnumItemClass.Block : EnumItemClass.Item)?.As<CollectibleObject>(parameterInfo.ParameterType);

    public static CollectibleObject? ProvideRaw(IServiceProvider serviceProvider, ParameterInfo parameterInfo, EContextualSource contextualSource, EnumItemClass itemType)
    {
        if (itemType == EnumItemClass.Block) return contextualSource switch
        {
            EContextualSource.Caller => serviceProvider.GetService<IWorldAccessor>().BlockAccessor.GetBlock(serviceProvider.GetService<Caller>().Entity.Required(contextualSource, parameterInfo.ParameterType).Pos.AsBlockPos.Down()),
            EContextualSource.CallerTarget => ContextualBlockSelectionProvider.Instance.Provide(serviceProvider, parameterInfo, contextualSource).GetOrFindBlock(serviceProvider.GetService<IWorldAccessor>()),
            _ => throw new NotSupportedException($"Unknown/Unsupported {nameof(EContextualSource)}: {contextualSource}"),
        };

        return ContextualItemStackProvider.Instance.Provide(serviceProvider, parameterInfo, contextualSource)?.Collectible;
    }
}
