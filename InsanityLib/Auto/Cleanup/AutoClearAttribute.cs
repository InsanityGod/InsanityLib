using InsanityLib.Constants;
using InsanityLib.Exceptions;
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
        foreach (var member in ReflectionUtil.FindAllMembersHavingAtribute<AutoClearAttribute>())
        {
            try
            {
                var value = member.GetValue();
                if(value is null) continue;

                var clearMethod = value.GetType().GetMethod("Clear");
                if(clearMethod is not null)
                {
                    clearMethod.AutoInvoke(provider, value);
                }
                else logger.Error(Logging.InvalidAttributeUsage, member.FindModName(), nameof(AutoClearAttribute), member.GetDebugDisplayName(), "Target type does not have a 'Clear' method");
            }
            catch(Exception ex)
            {
                logger.Error(Logging.ExecutionFailed, nameof(AutoClearAttribute), member, ex);
            }
        }
    }
}
