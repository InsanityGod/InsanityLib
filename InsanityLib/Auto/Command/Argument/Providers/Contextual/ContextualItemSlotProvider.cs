using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
public sealed class ContextualItemSlotProvider : IContextualArgumentProvider<ItemSlot?>
{
    private ContextualItemSlotProvider() { }

    public static readonly ContextualItemSlotProvider Instance = new();

    public EContextualSource DefaultSource(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => EContextualSource.Caller;

    public ItemSlot? Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, EContextualSource contextualSource) => 
        (ContextualEntityProvider.Instance.Provide(serviceProvider, argumentInfo, contextualSource) as EntityAgent)?.ActiveHandItemSlot;
}
