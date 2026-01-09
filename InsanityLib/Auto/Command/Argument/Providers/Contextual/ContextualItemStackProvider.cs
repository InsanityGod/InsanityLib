using InsanityLib.Extensions;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
#nullable enable
public sealed class ContextualItemStackProvider : IContextualArgumentProvider<ItemStack?>
{
    private ContextualItemStackProvider() { }

    public static readonly ContextualItemStackProvider Instance = new();

    public EContextualSource DefaultSource(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => ContextualItemSlotProvider.Instance.DefaultSource(serviceProvider, parameterInfo);

    public ItemStack? Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, EContextualSource contextualSource) => 
        ContextualItemSlotProvider.Instance.Provide(serviceProvider, parameterInfo, contextualSource)?.Itemstack;
}
