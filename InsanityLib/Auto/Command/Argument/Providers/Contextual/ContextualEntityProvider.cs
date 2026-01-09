using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
#nullable enable
public sealed class ContextualEntityProvider : IContextualArgumentProvider<Entity?>
{
    private ContextualEntityProvider() { }
    
    public static readonly ContextualEntityProvider Instance = new();

    public bool CanProvide(ParameterInfo paramInfo, CommandParameterAttribute? attr) => typeof(Entity).IsAssignableFrom(paramInfo.ParameterType);

    public EContextualSource DefaultSource(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => typeof(EntityPlayer).IsAssignableFrom(parameterInfo.ParameterType) ? EContextualSource.Caller : EContextualSource.CallerTarget;

    public Entity? Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, EContextualSource contextualSource) =>
        ProvideRaw(serviceProvider, parameterInfo, contextualSource)?.As<Entity>(parameterInfo.ParameterType);

    public static Entity? ProvideRaw (IServiceProvider serviceProvider, ParameterInfo parameterInfo, EContextualSource contextualSource) => contextualSource switch
    {
        EContextualSource.Caller => serviceProvider.GetService<Caller>().Entity.Required(contextualSource, parameterInfo.ParameterType),
        EContextualSource.CallerTarget => serviceProvider.GetService<Caller>().Entity.Required(contextualSource, parameterInfo.ParameterType).GetTargetEntity()?.Entity,
        _ => throw new NotSupportedException($"Unknown/Unsupported {nameof(EContextualSource)}: {contextualSource}"),
    };
}
