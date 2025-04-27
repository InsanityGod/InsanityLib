using InsanityLib.Attributes.Auto;
using InsanityLib.UI;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using System;
using System.ComponentModel.Design;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using InsanityLib.Attributes;
using InsanityLib.Extended;
using InsanityLib.Util.ContentFeatures;
using Vintagestory.API.Datastructures;




#if DEBUG
using InsanityLib.UI.Examples;
#endif

[assembly: AutoPatcher("insanitylib")]
[assembly: AutoRegistry("insanitylib")]
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
            EnumExtensionUtil.EnumExtensions[typeof(EnumTransitionType)] = new ExtendedTransition();
            AssetCategoryAttribute.Load();
            AutoRegistryAttribute.RegisterAll(api);
            AutoConfig.LoadAll(ServiceContainer);
        }

        public override void AssetsLoaded(ICoreAPI api)
        {
            CustomTransition.LoadAssets(api);
        }

        public override void Start(ICoreAPI api)
        {
            api.RegisterAutoCommands();
            AutoPatcherAttribute.AutoPatch(api);

            //Clear documentation cache build up by auto registration code
            DocumentationUtil.ClearCache();
        }
        public override void StartClientSide(ICoreClientAPI api)
        {

            ServiceContainer.CollectAutoGuiComposers();

            #if DEBUG //Example UI
                api.Input.RegisterHotKey("insanitylib:toggleAutoGui", "AutoGuiTest", GlKeys.Home, HotkeyType.GUIOrOtherControls);
                api.Input.GetHotKeyByCode("insanitylib:toggleAutoGui").Handler += (hotkey) => new AutoGuiDialog(api, new ExampleUI()).TryOpen();
            #endif
        }

        //TODO remove test code
        public override void AssetsFinalize(ICoreAPI api)
        {
            var testValue = EnumTransitionType.None + 1;

            var result = CustomTransition.ExtendedEnum.FindHandler(testValue);

            base.AssetsFinalize(api);
        }

        public override void Dispose()
        {
            DisposalLogicAttribute.DisposeAll(ServiceContainer);
        }
    }
}
