using InsanityLib.Constants;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory;
using Vintagestory.API.Common;

namespace InsanityLib.Attributes.Auto
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DisposalLogicAttribute : Attribute
    {
        public int Priority { get; init; }

        internal static void DisposeAll(IServiceContainer serviceContainer)
        {
            foreach ((var member, var attr) in ReflectionUtil.FindAllMembers<DisposalLogicAttribute>().OrderBy(pair => pair.Item2.Priority))
            {
                try
                {
                    member.AutoInvoke(serviceContainer);
                }
                catch (Exception ex)
                {
                    serviceContainer.GetService<ILogger>()?.Error(Logging.ExecutionFailedTemplate, nameof(DisposeAll), member, ex);
                }
            }

            //TODO dispose all IDisposable services that actually come from mods
        }
    }
}
