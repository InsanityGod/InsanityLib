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

    public bool CanProvide(IArgumentInfo argumentInfo, CommandParameterAttribute? attr) => true;

    public object? Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, TextCommandCallingArgs currentArgs, ref int consumedParsers)
    {
        var defaultAttr = argumentInfo.GetCustomAttribute<DefaultValueAttribute>();
        
        if(defaultAttr is AutoDefaultValueAttribute autoDefaultAttr)
        {
            return autoDefaultAttr.GetAutoDefaultValue(serviceProvider, null);
        }
        else if(defaultAttr is not null) return defaultAttr.Value;

        return argumentInfo.HasDefaultValue ? argumentInfo.DefaultValue : argumentInfo.Type.Default();
    }
}
