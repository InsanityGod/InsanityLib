using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vintagestory.GameContent;

namespace InsanityLib.Auto.Command.Argument;

public sealed class InstanceArgumentInfo(Type type) : IArgumentInfo
{

    public bool HasDefaultValue => false;
    public object? DefaultValue => null;

    public Type Type => type;

    public string DebugName => "instance/this";

    public string GetHumanReadableName() => "Target";

    public T? GetCustomAttribute<T>(bool inherit = true) where T : Attribute => null; //Not relevant for argument info

    public IEnumerable<T> GetCustomAttributes<T>(bool inherit = true) where T : Attribute => []; //Not relevant for argument info

    public void Validate(IServiceProvider serviceProvider, object? value)
    {
        if(value is null)
        {
            throw new ValidationException("instance is null");
        }
    }
}
