using InsanityLib.Extensions;
using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
public sealed class ContextualEntityProvider : IContextualArgumentProvider<Entity?>
{
    private ContextualEntityProvider() { }
    
    public static readonly ContextualEntityProvider Instance = new();

    public bool CanProvide(IArgumentInfo argumentInfo, CommandParameterAttribute? attr) => typeof(Entity).IsAssignableFrom(argumentInfo.Type);

    public EContextualSource DefaultSource(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => typeof(EntityPlayer).IsAssignableFrom(argumentInfo.Type) ? EContextualSource.Caller : EContextualSource.CallerTarget;

    public Entity? Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, EContextualSource contextualSource) =>
        ProvideRaw(serviceProvider, argumentInfo, contextualSource)?.As<Entity>(argumentInfo.Type);

    public static Entity? ProvideRaw (IServiceProvider serviceProvider, IArgumentInfo argumentInfo, EContextualSource contextualSource) => contextualSource switch
    {
        EContextualSource.Caller => serviceProvider.GetService<Caller>()!.Entity.Required(contextualSource, argumentInfo.Type),
        EContextualSource.CallerTarget => serviceProvider.GetService<Caller>()!.Entity.Required(contextualSource, argumentInfo.Type).GetTargetEntity()?.Entity,
        _ => throw new NotSupportedException($"Unknown/Unsupported {nameof(EContextualSource)}: {contextualSource}"),
    };
}
