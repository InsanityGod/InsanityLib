using HarmonyLib;
using InsanityLib.Constants;
using InsanityLib.Generators.Attributes;
using InsanityLib.Util;
using System;
using System.ComponentModel;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Cleanup;

/// <summary>
/// Automatically assigns a new "default" value of a static field or property when the disposal logic runs. <br />
/// (for non static members this provides metadata about the default value, similar to the normal DefaultValueAttribute attribute)
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class AutoDefaultValueAttribute(object? value = null) : DefaultValueAttribute(value)
{
    /// <summary>
    /// If provided an instance of this type will be created to set the value. <br />
    /// If <see cref="AutoMethodName"/> is also set, this will be used as the class to search for the method on.
    /// </summary>
    public Type? AutoType { get; init; }

    /// <summary>
    /// If provided, this method will be used to set the value. <br />
    /// If <see cref="AutoType"/> is also set, this will be used as the methodname to search for on the type.
    /// </summary>
    public string? AutoMethodName { get; init; }

    [DisposalLogic(ExecutionOrder = int.MinValue)]
    internal static void DefaultAll(IServiceProvider serviceContainer, ILogger logger)
    {
        foreach ((var member, var attr) in ReflectionUtil.FindAllMembersWithAttributes<AutoDefaultValueAttribute>())
        {
            if(!member.IsStatic()) continue;
            try
            {
                member.SetAutoDefaultValue(attr, null, serviceContainer);
            }
            catch(Exception ex)
            {
                logger.Error(Logging.AutoDefaultFailed, member, ex);
            }
        }
    }

    /// <summary>
    /// Automatically creates a default value based on the attribute settings
    /// </summary>
    /// <returns>A new default value</returns>
    /// <exception cref="InvalidOperationException" />
    public object? GetAutoDefaultValue(IServiceProvider provider, object? instance)
    {
        if (!string.IsNullOrEmpty(AutoMethodName))
        {
            var method = (AutoType is null ?
                AccessTools.Method(AutoMethodName) :
                AccessTools.Method(AutoType, AutoMethodName))
                ?? throw new InvalidOperationException($"Combination of AutoMethodName: '{AutoMethodName}' and AutoType: '{AutoType}' could not be resolved");

            return method.AutoInvoke(provider, method.DeclaringType?.IsInstanceOfType(instance) ?? false ? instance : null);
        }

        if(AutoType is not null) return AutoType.AutoCreate(provider, false);

        return Value;
    }
}
