using InsanityLib.Auto.Cleanup;
using InsanityLib.Extensions;
using System;
using System.ComponentModel;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Special;
public sealed class DefaultArgumentProvider : ICommandArgumentProvider
{
    private DefaultArgumentProvider() { }

    public static readonly DefaultArgumentProvider Instance = new();

    public bool CanProvide(ParameterInfo paramInfo, CommandParameterAttribute? attr) => true;

    public object? Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, TextCommandCallingArgs currentArgs, ref int consumedParsers)
    {
        var defaultAttr = parameterInfo.GetCustomAttribute<DefaultValueAttribute>();
        
        if(defaultAttr is AutoDefaultValueAttribute autoDefaultAttr)
        {
            return autoDefaultAttr.GetAutoDefaultValue(serviceProvider, null);
        }
        else if(defaultAttr is not null) return defaultAttr.Value;

        return parameterInfo.HasDefaultValue ? parameterInfo.DefaultValue : parameterInfo.ParameterType.Default();
    }
}
