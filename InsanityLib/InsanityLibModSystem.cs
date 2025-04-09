using InsanityLib.Attributes.Auto;
using InsanityLib.Attributes.Auto.Harmony;
using InsanityLib.Config;
using InsanityLib.Constants;
using InsanityLib.UI;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using System;
using System.ComponentModel.Design;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

[assembly:AutoPatcher("insanitylib")]
namespace InsanityLib
{
    public class InsanityLibModSystem : ModSystem, IServiceProvider
    {
        [AutoDefaultValue(defaultInstanceType:typeof(ServiceContainer))]
        public static IServiceContainer GlobalServiceContainer { get; private set; } = new ServiceContainer(); //TODO write a custom service container

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
            
            AutoConfigAttribute.LoadAll(ServiceContainer);
        }

        public override void Start(ICoreAPI api)
        {
            api.RegisterAutoCommands();
            api.AutoPatch();
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            
            //TODO
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            ServiceContainer.CollectAutoGuiComposers();

            #if DEBUG
                api.Input.RegisterHotKey("insanitylib:toggleAutoGui", "AutoGuiTest", GlKeys.Home, HotkeyType.GUIOrOtherControls);
                api.Input.GetHotKeyByCode("insanitylib:toggleAutoGui").Handler += (hotkey) => api.OpenAutoGui(InsanityLibConfig.Instance);
            #endif
            //TODO
            //TODO local player in service collection
        }


        public override void Dispose() => DisposalLogicAttribute.DisposeAll(ServiceContainer);
    }
}
