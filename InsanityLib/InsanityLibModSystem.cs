using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using System;
using System.ComponentModel.Design;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using InsanityLib.Util.ContentFeatures;
using InsanityLib.Documentation;
using InsanityLib.Auto.Command;
using InsanityLib.Auto.Setup;
using InsanityLib.Auto.Cleanup;
using InsanityLib.Extended.AssetCategories;
using InsanityLib.Extended.Transitions;

[assembly: AutoPatcher("insanitylib")]
[assembly: AutoRegistry("insanitylib")]
namespace InsanityLib;

public class InsanityLibModSystem : ModSystem, IServiceProvider
{
    [AutoDefaultValue(AutoType = typeof(ServiceContainer))]
    public static IServiceContainer GlobalServiceContainer { get; private set; } = new ServiceContainer(); //TODO write a custom service container
    
    public IServiceContainer ServiceContainer { get; private set; }

    public object GetService(Type serviceType) => ServiceContainer.GetService(serviceType);

    public override void StartPre(ICoreAPI api)
    {
        ReflectionUtil.LoadedSides ??= api.Side;
        ReflectionUtil.LoadedSides |= api.Side;
        if (api is ICoreClientAPI clientApi) GlobalServiceContainer.Register(clientApi);
        if (api is ICoreServerAPI serverApi) GlobalServiceContainer.Register(serverApi);
        
        ServiceContainer = new ServiceContainer(GlobalServiceContainer);
        ServiceContainer.Register(api);
        ServiceContainer.Register(api.World);
        ServiceContainer.Register(api.Logger);
        EnumExtensionUtil.EnumExtensions[typeof(EnumTransitionType)] = new ExtendedTransition();
        AssetCategoryAttribute.Load();
        AutoRegistryAttribute.RegisterAll(api); //TODO see about allowing for config values to be used in patching
        AutoConfigUtil.LoadAll(api);
        AutoPatcherAttribute.AutoPatch(api);
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        CustomTransition.LoadAssets(api);
    }

    public override void Start(ICoreAPI api)
    {
        api.RegisterAutoCommands();

        //Clear documentation cache build up by auto registration code
        AssemblyDocumentationContext.ClearCache();
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        ServiceContainer.CollectAutoGuiComposers();
    }

    public override void Dispose()
    {
        DisposalLogicAttribute.DisposeAll(ServiceContainer);
        
        if(ServiceContainer is null) return;
        var api = ServiceContainer.GetService<ICoreAPI>();
        if(api is not null && ReflectionUtil.LoadedSides is not null)
        {
            ReflectionUtil.LoadedSides &= ~api.Side;
        }
    }
}
