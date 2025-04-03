using InsanityLib.Attributes.Auto;
using InsanityLib.Constants;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace InsanityLib
{
    public class InsanityLibModSystem : ModSystem, IServiceProvider
    {
        [AutoDefaultValue(defaultInstanceType:typeof(ServiceContainer))]
        public static IServiceContainer GlobalServiceContainer { get; private set; } = new ServiceContainer();

        public IServiceContainer ServiceContainer { get; private set; }

        public object GetService(Type serviceType) => ServiceContainer.GetService(serviceType);

        public override void StartPre(ICoreAPI api)
        {
            ReflectionUtil.LoadedSides ??= api.Side;
            ReflectionUtil.LoadedSides &= api.Side;
            if (api is ICoreClientAPI clientApi) GlobalServiceContainer.Register(clientApi);
            if (api is ICoreServerAPI serverApi) GlobalServiceContainer.Register(serverApi);
            
            ServiceContainer = new ServiceContainer(GlobalServiceContainer);
            ServiceContainer.Register(api);
            ServiceContainer.Register(api.World);
            ServiceContainer.Register(api.Logger);
        }

        public override void Start(ICoreAPI api)
        {
            api.RegisterAutoCommands();
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            
            //TODO
        }

        public override void StartClientSide(ICoreClientAPI api)
        {

            //TODO
            //TODO local player in service collection
        }


        public override void Dispose()
        {
            var logger = ServiceContainer.GetService<ILogger>();
            foreach ((var member, var attr) in ReflectionUtil.FindAllMembers<DisposalLogicAttribute>().OrderBy(pair => pair.Item2.Priority))
            {
                try
                {
                    member.AutoInvoke(ServiceContainer);
                }
                catch (Exception ex)
                {
                    logger?.Error(Logging.ExecutionFailedTemplate, nameof(Dispose), member, ex);
                }
            }

            //Doing this one manual (as this one needs to persist until we are compleetly done)
            ServiceContainer = null;
        }
    }
}
