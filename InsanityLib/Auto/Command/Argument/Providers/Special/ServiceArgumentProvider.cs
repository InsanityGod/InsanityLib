using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Special;
#nullable enable
public sealed class ServiceArgumentProvider : ICommandArgumentProvider
{
    private ServiceArgumentProvider() { }

    public static readonly ServiceArgumentProvider Instance = new();

    public bool CanProvide(ParameterInfo paramInfo, CommandParameterAttribute? attr) => !paramInfo.ParameterType.IsValueType && paramInfo.ParameterType != typeof(string);

    public object? Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, TextCommandCallingArgs currentArgs, ref int consumedParsers) => serviceProvider.GetService(parameterInfo.ParameterType);
}
