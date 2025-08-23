using HarmonyLib;
using InsanityLib.Constants;
using InsanityLib.Util;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using Vintagestory.API.Common;

namespace InsanityLib.Attributes.Auto;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class AutoDefaultValueAttribute : DefaultValueAttribute
{
    /// <summary>
    /// If provided an instance of this type will be created to set the value. <br />
    /// If <see cref="AutoMethodName"/> is also set, this will be used as the class to search for the method on.
    /// </summary>
    public Type AutoType { get; init; }

    /// <summary>
    /// If provided, this method will be used to set the value. <br />
    /// If <see cref="AutoType"/> is also set, this will be used as the methodname to search for on the type.
    /// </summary>
    public string AutoMethodName { get; init; }
    public AutoDefaultValueAttribute(object value = null) : base(value) { }

    [DisposalLogic(Priority = int.MinValue)]
    internal static void DefaultAll(IServiceContainer serviceContainer)
    {
        foreach ((var member, var attr) in ReflectionUtil.FindAllMembers<AutoDefaultValueAttribute>())
        {
            if(!member.IsStatic()) continue;
            try
            {
                member.SetAutoDefaultValue(attr, null, serviceContainer);
            }
            catch(Exception ex)
            {
                serviceContainer.GetService<ILogger>()?.Error(Logging.AutoDefaultFailed, member, ex);
            }
        }
    }

    public object GetAutoDefaultValue(IServiceProvider provider, object instance)
    {
        if (!string.IsNullOrEmpty(AutoMethodName))
        {
            var method = (AutoType is null ?
                AccessTools.Method(AutoMethodName) :
                AccessTools.Method(AutoType, AutoMethodName))
                ?? throw new InvalidOperationException($"Combination of AutoMethodName: '{AutoMethodName}' and AutoType: '{AutoType}' could not be resolved");

            return method.AutoInvoke(provider, (method.DeclaringType?.IsInstanceOfType(instance) ?? false) ? instance : null);
        }
        if(AutoType is not null)
        {
            return AutoType.AutoCreate(provider, false);
        }

        return Value;
    }
}
