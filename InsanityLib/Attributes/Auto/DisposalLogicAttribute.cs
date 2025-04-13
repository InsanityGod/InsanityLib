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

        /// <summary>
        /// Wether the logic is allowed to run twice
        /// </summary>
        public bool MayRunTwice { get; init; } = false;

        /// <summary>
        /// The side on which the disposal logic should run. <br />
        /// If set to <see cref="EnumAppSide.Universal"/>, the logic is allowed to run on either side but will only run on twice if <seealso cref="MayRunTwice"/> is set to true.
        /// </summary>
        public EnumAppSide Side { get; init; } = EnumAppSide.Universal;

        internal static void DisposeAll(IServiceContainer serviceContainer)
        {
            var api = serviceContainer.GetService<ICoreAPI>();
            var loadedSides = ReflectionUtil.LoadedSides.Value;
            foreach ((var member, var attr) in ReflectionUtil.FindAllMembers<DisposalLogicAttribute>().OrderBy(pair => pair.Item2.Priority))
            {
                try
                {
                    //TODO test this
                    if((attr.Side & api.Side) == 0) continue; //If the current api side does not match the Side it's allowed to run on
                    if (!attr.MayRunTwice && attr.Side == EnumAppSide.Universal && loadedSides == EnumAppSide.Universal && api.Side != EnumAppSide.Server) continue;
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
