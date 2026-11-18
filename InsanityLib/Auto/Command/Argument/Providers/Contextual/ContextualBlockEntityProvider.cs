using InsanityLib.Extensions;
using System;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
public sealed class ContextualBlockEntityProvider : IContextualArgumentProvider<BlockEntity?>
{
    private ContextualBlockEntityProvider() { }
    
    public static readonly ContextualBlockEntityProvider Instance = new();

    public BlockEntity? Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, EContextualSource contextualSource) =>
        ProvideRaw(serviceProvider, argumentInfo, contextualSource)?.As<BlockEntity>(argumentInfo.Type);

    public static BlockEntity? ProvideRaw(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, EContextualSource contextualSource)
    {
        var pos = ContextualBlockPosProvider.Instance.Provide(serviceProvider, argumentInfo, contextualSource);
        if (pos is null) return null;
        return serviceProvider.GetService<IWorldAccessor>()!.BlockAccessor.GetBlockEntity(pos);
    }
}
