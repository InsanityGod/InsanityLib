using HarmonyLib;
using InsanityLib.Constants;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace InsanityLib.Attributes.Auto
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AutoClearAttribute : Attribute
    {
        //Auto bind this logic to dispose method
        [DisposalLogic(Priority = int.MinValue)]
        internal static void ClearAll(ILogger logger)
        {
            foreach ((var member, _) in ReflectionUtil.FindAllMembers<AutoClearAttribute>())
            {
                try
                {
                    var value = member.GetValue();
                    if(value == null) continue;

                    var clearMethod = value.GetType().GetMethod("Clear");
                    clearMethod.Invoke(value, null);
                }
                catch(Exception ex)
                {
                    logger?.Error(Logging.ExecutionFailedTemplate, nameof(AutoClearAttribute), member, ex);
                }
            }
        }
    }
}
