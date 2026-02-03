using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers;
public interface IContextualArgumentProvider<out T> : ICommandArgumentProvider<T>
{
    object? ICommandArgumentProvider.Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, TextCommandCallingArgs currentArgs, ref int consumedParsers) 
        => Provide(serviceProvider, parameterInfo, parameterInfo.GetCustomAttribute<CommandParameterAttribute>()?.ContextualSource ?? DefaultSource(serviceProvider, parameterInfo));
    
    T Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, EContextualSource contextualSource);

    EContextualSource DefaultSource(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => EContextualSource.CallerTarget;
}
