using InsanityLib.Attributes.Auto;
using InsanityLib.Attributes.Auto.Harmony;
using InsanityLib.Config;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using System;
using System.ComponentModel.Design;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

[assembly: AutoPatcher("insanitylib")]
namespace InsanityLib
{
    public class InsanityLibModSystem : ModSystem, IServiceProvider
    {
        [AutoDefaultValue(AutoType = typeof(ServiceContainer))]
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
            
            AutoConfig.LoadAll(ServiceContainer);
        }

        public override void Start(ICoreAPI api)
        {
            api.RegisterAutoCommands();
            api.AutoPatch();

            //Clear documentation cache build up by auto registration code
            DocumentationUtil.ClearCache();
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            ServiceContainer.CollectAutoGuiComposers();

            #if DEBUG //TODO Cleanup once finished creating/testing AutoGui
                api.Input.RegisterHotKey("insanitylib:toggleAutoGui", "AutoGuiTest", GlKeys.Home, HotkeyType.GUIOrOtherControls);
                api.Input.GetHotKeyByCode("insanitylib:toggleAutoGui").Handler += (hotkey) => api.AutoGui(InsanityLibConfig.Instance).TryOpen();
            #endif
        }

        public override void Dispose()
        {
            DisposalLogicAttribute.DisposeAll(ServiceContainer);
        }
    }
}
