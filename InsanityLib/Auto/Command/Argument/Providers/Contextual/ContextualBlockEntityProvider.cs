using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
public sealed class ContextualBlockEntityProvider : IContextualArgumentProvider<BlockEntity?>
{
    private ContextualBlockEntityProvider() { }
    
    public static readonly ContextualBlockEntityProvider Instance = new();

    public BlockEntity? Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, EContextualSource contextualSource) =>
        ProvideRaw(serviceProvider, parameterInfo, contextualSource)?.As<BlockEntity>(parameterInfo.ParameterType);

    public static BlockEntity? ProvideRaw(IServiceProvider serviceProvider, ParameterInfo parameterInfo, EContextualSource contextualSource)
    {
        var pos = ContextualBlockPosProvider.Instance.Provide(serviceProvider, parameterInfo, contextualSource);
        if (pos is null) return null;
        return serviceProvider.GetService<IWorldAccessor>().BlockAccessor.GetBlockEntity(pos);
    }
}
