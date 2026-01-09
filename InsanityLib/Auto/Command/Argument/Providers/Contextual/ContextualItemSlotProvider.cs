using InsanityLib.Extensions;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
#nullable enable
public sealed class ContextualItemSlotProvider : IContextualArgumentProvider<ItemSlot?>
{
    private ContextualItemSlotProvider() { }

    public static readonly ContextualItemSlotProvider Instance = new();

    public EContextualSource DefaultSource(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => EContextualSource.Caller;

    public ItemSlot? Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, EContextualSource contextualSource) => 
        (ContextualEntityProvider.Instance.Provide(serviceProvider, parameterInfo, contextualSource) as EntityAgent)?.ActiveHandItemSlot;
}
