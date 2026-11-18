using InsanityLib.Extensions;
using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
public sealed class ContextualBlockPosProvider : IContextualArgumentProvider<BlockPos?>
{
    private ContextualBlockPosProvider() { }
    
    public static readonly ContextualBlockPosProvider Instance = new();

    public BlockPos? Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, EContextualSource contextualSource) => contextualSource switch
    {
        EContextualSource.Caller => serviceProvider.GetService<Caller>()!.Pos.PosRequired().AsBlockPos,
        EContextualSource.CallerTarget =>  ContextualBlockSelectionProvider.Instance.Provide(serviceProvider, argumentInfo, EContextualSource.Caller)?.Position,
        _ => throw new NotSupportedException($"Unknown/Unsupported {nameof(EContextualSource)}: {contextualSource}"),
    };
}
