using InsanityLib.Constants;
using InsanityLib.Util;
using System;
using System.ComponentModel.Design;
using Vintagestory.API.Common;

namespace InsanityLib.Attributes.Auto
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AutoClearAttribute : Attribute
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
}
