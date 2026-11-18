using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace InsanityLib.Auto.Command.Argument;

public sealed class ParameterArgumentInfo(ParameterInfo info) : IArgumentInfo
{

    public bool HasDefaultValue => info.HasDefaultValue;
    
    public object? DefaultValue => info.DefaultValue;

    public Type Type => info.ParameterType;

    public string DebugName => info.Name!;

    public string GetHumanReadableName() => info.GetHumanReadableName();

    public T? GetCustomAttribute<T>(bool inherit = true) where T : Attribute => info.GetCustomAttribute<T>(inherit);
    public IEnumerable<T> GetCustomAttributes<T>(bool inherit = true) where T : Attribute => info.GetCustomAttributes<T>(inherit);

    public void Validate(IServiceProvider serviceProvider, object? value)
    {
        if (!info.TryValidate(serviceProvider, value, out var exception))
        {
            throw exception!;
        }
    }
}
