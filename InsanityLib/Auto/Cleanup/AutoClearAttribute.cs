using InsanityLib.Constants;
using InsanityLib.Util;
using System;
using System.ComponentModel.Design;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Cleanup;

/// <summary>
/// Automatically calls the "Clear" method of a static field or property when the disposal logic runs.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class AutoClearAttribute : Attribute
{
    [DisposalLogic(Priority = int.MinValue)]
    internal static void ClearAll(IServiceContainer provider, ILogger logger)
    {
        foreach ((var member, _) in ReflectionUtil.FindAllMembers<AutoClearAttribute>())
        {
            try
            {
                var value = member.GetValue();
                if(value is null) continue;

                var clearMethod = value.GetType().GetMethod("Clear");
                clearMethod.AutoInvoke(provider, value);
            }
            catch(Exception ex)
            {
                logger?.Error(Logging.ExecutionFailedTemplate, nameof(AutoClearAttribute), member, ex);
            }
        }
    }
}
