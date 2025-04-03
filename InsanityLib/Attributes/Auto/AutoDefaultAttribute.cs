using HarmonyLib;
using InsanityLib.Constants;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace InsanityLib.Attributes.Auto
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AutoDefaultValueAttribute : DefaultValueAttribute
    {
        public readonly Type DefaultInstanceType;
        public AutoDefaultValueAttribute(object value = null, Type defaultInstanceType = null) : base(value)
        {
            DefaultInstanceType = defaultInstanceType;
        }

        //Auto bind this logic to dispose method
        [DisposalLogic(int.MinValue)]
        internal static void ClearAll(ILogger logger)
        {
            foreach ((var member, var attr) in ReflectionUtil.FindAllMembers<AutoDefaultValueAttribute>())
            {
                try
                {
                    if (attr.DefaultInstanceType != null)
                    {
                        member.SetValue(Activator.CreateInstance(attr.DefaultInstanceType));
                    }
                    else member.SetValue(attr.Value);
                }
                catch(Exception ex)
                {
                    logger?.Error(Logging.ExecutionFailedTemplate, nameof(AutoDefaultValueAttribute), member, ex);
                }
            }
        }
    }
}
