using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
public sealed class ContextualItemStackProvider : IContextualArgumentProvider<ItemStack?>
{
    private ContextualItemStackProvider() { }

    public static readonly ContextualItemStackProvider Instance = new();

    public EContextualSource DefaultSource(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => ContextualItemSlotProvider.Instance.DefaultSource(serviceProvider, argumentInfo);

    public ItemStack? Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, EContextualSource contextualSource) => 
        ContextualItemSlotProvider.Instance.Provide(serviceProvider, argumentInfo, contextualSource)?.Itemstack;
}
