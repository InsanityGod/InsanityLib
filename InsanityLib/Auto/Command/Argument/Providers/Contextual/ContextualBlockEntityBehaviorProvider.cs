using System;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
public sealed class ContextualBlockEntityBehaviorProvider : IContextualArgumentProvider<BlockEntityBehavior?>
{
    private ContextualBlockEntityBehaviorProvider() { }
    
    public static readonly ContextualBlockEntityBehaviorProvider Instance = new();

    public bool CanProvide(IArgumentInfo argumentInfo, CommandParameterAttribute? attr) => typeof(BlockEntityBehavior).IsAssignableFrom(argumentInfo.Type);

    public BlockEntityBehavior? Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, EContextualSource contextualSource) => 
        ContextualBlockEntityProvider.ProvideRaw(serviceProvider, argumentInfo, contextualSource)?.Behaviors.FirstOrDefault(argumentInfo.Type.IsInstanceOfType);
}
