using InsanityLib.Extensions;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Contextual;
public sealed class ContextualPlayerProvider : IContextualArgumentProvider<IPlayer?>
{
    private ContextualPlayerProvider() { }

    public static readonly ContextualPlayerProvider Instance = new();

    public EContextualSource DefaultSource(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => EContextualSource.Caller;

    public IPlayer? Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, EContextualSource contextualSource) => contextualSource switch
    {
        EContextualSource.Caller => serviceProvider.GetService<Caller>()!.Player.Required(contextualSource, argumentInfo.Type),
        EContextualSource.CallerTarget => serviceProvider.GetService<Caller>()!.Entity.Required(contextualSource, argumentInfo.Type).GetTargetEntity()?.Entity.As<EntityPlayer>(argumentInfo.Type)?.Player,
        _ => throw new NotSupportedException($"Unknown/Unsupported {nameof(EContextualSource)}: {contextualSource}"),
    };
}
