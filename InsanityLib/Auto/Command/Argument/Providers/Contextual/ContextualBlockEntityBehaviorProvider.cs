using System;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
#nullable enable
public sealed class ContextualBlockEntityBehaviorProvider : IContextualArgumentProvider<BlockEntityBehavior?>
{
    private ContextualBlockEntityBehaviorProvider() { }
    
    public static readonly ContextualBlockEntityBehaviorProvider Instance = new();

    public bool CanProvide(ParameterInfo paramInfo, CommandParameterAttribute? attr) => typeof(CollectibleBehavior).IsAssignableFrom(paramInfo.ParameterType);

    public BlockEntityBehavior? Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, EContextualSource contextualSource) => 
        ContextualBlockEntityProvider.ProvideRaw(serviceProvider, parameterInfo, contextualSource)?.Behaviors.FirstOrDefault(parameterInfo.ParameterType.IsInstanceOfType);
}
