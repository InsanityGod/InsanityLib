using System;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
public sealed class ContextualEntityBehaviorProvider : IContextualArgumentProvider<EntityBehavior?>
{
    private ContextualEntityBehaviorProvider() { }
    
    public static readonly ContextualEntityBehaviorProvider Instance = new();

    public bool CanProvide(IArgumentInfo argumentInfo, CommandParameterAttribute? attr) => typeof(EntityBehavior).IsAssignableFrom(argumentInfo.Type);

    public EntityBehavior? Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, EContextualSource contextualSource) => 
        ContextualEntityProvider.ProvideRaw(serviceProvider, argumentInfo, contextualSource)?.SidedProperties.Behaviors.FirstOrDefault(argumentInfo.Type.IsInstanceOfType);
}
