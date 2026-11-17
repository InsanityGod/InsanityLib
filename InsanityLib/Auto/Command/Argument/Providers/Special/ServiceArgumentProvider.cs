using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Special;
public sealed class ServiceArgumentProvider : ICommandArgumentProvider
{
    private ServiceArgumentProvider() { }

    public static readonly ServiceArgumentProvider Instance = new();

    public bool CanProvide(IArgumentInfo argumentInfo, CommandParameterAttribute? attr) => !argumentInfo.Type.IsValueType && argumentInfo.Type != typeof(string);

    public object? Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, TextCommandCallingArgs currentArgs, ref int consumedParsers) => serviceProvider.GetService(argumentInfo.Type);
}
