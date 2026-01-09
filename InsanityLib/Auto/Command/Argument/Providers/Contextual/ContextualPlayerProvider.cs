using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
#nullable enable
public sealed class ContextualPlayerProvider : IContextualArgumentProvider<IPlayer?>
{
    private ContextualPlayerProvider() { }

    public static readonly ContextualPlayerProvider Instance = new();

    public EContextualSource DefaultSource(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => EContextualSource.Caller;

    public IPlayer? Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, EContextualSource contextualSource) => contextualSource switch
    {
        EContextualSource.Caller => serviceProvider.GetService<Caller>().Player.Required(contextualSource, parameterInfo.ParameterType),
        EContextualSource.CallerTarget => serviceProvider.GetService<Caller>().Entity.Required(contextualSource, parameterInfo.ParameterType).GetTargetEntity()?.Entity.As<EntityPlayer>(parameterInfo.ParameterType)?.Player,
        _ => throw new NotSupportedException($"Unknown/Unsupported {nameof(EContextualSource)}: {contextualSource}"),
    };
}
