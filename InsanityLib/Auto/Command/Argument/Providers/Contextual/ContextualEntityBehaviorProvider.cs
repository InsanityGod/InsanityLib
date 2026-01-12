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

    public bool CanProvide(ParameterInfo paramInfo, CommandParameterAttribute? attr) => typeof(CollectibleBehavior).IsAssignableFrom(paramInfo.ParameterType);

    public EntityBehavior? Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, EContextualSource contextualSource) => 
        ContextualEntityProvider.ProvideRaw(serviceProvider, parameterInfo, contextualSource)?.SidedProperties.Behaviors.FirstOrDefault(parameterInfo.ParameterType.IsInstanceOfType);
}
