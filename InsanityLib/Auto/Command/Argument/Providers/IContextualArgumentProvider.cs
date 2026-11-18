using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers;
public interface IContextualArgumentProvider<out T> : ICommandArgumentProvider<T>
{
    object? ICommandArgumentProvider.Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, TextCommandCallingArgs currentArgs, ref int consumedParsers) 
        => Provide(serviceProvider, argumentInfo, argumentInfo.GetCustomAttribute<CommandParameterAttribute>()?.ContextualSource ?? DefaultSource(serviceProvider, argumentInfo));
    
    T Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, EContextualSource contextualSource);

    EContextualSource DefaultSource(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => EContextualSource.CallerTarget;
}
