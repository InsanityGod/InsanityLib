using System;
using System.Collections.Generic;

namespace InsanityLib.Auto.Command.Argument;

public interface IArgumentInfo
{
    bool HasDefaultValue { get; }

    object? DefaultValue { get; }

    string DebugName { get; }

    Type Type { get; }

    T? GetCustomAttribute<T>(bool inherit = true) where T : Attribute;
    IEnumerable<T> GetCustomAttributes<T>(bool inherit = true) where T : Attribute;
    string GetHumanReadableName();
    void Validate(IServiceProvider serviceProvider, object? value);
}
