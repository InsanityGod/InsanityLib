using InsanityLib.Auto.Cleanup;
using InsanityLib.Auto.Command;
using InsanityLib.Auto.Setup;
using InsanityLib.Documentation;
using InsanityLib.Extended.AssetCategories;
using InsanityLib.Extended.Transitions;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using InsanityLib.Util.ContentFeatures;
using System;
using System.ComponentModel.Design;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

[assembly: AutoPatcher("insanitylib")]
[assembly: AutoRegistry("insanitylib")]
namespace InsanityLib;

public class InsanityLibModSystem : ModSystem, IServiceProvider
{
    public static IServiceContainer GlobalServiceContainer { get; private set; } = new ServiceContainer();
    
    public IServiceContainer ServiceContainer { get; } = new ServiceContainer(GlobalServiceContainer);

    private ICoreAPI _api;

    public object GetService(Type serviceType)
    {
        var result = ServiceContainer.GetService(serviceType);
        if (result is not null) return result;

        foreach (var modSystem in _api.ModLoader.Systems)
        {
            if(serviceType.IsInstanceOfType(modSystem))
            {
                return modSystem;
            }
        }

        return null;
    }

    public override void StartPre(ICoreAPI api)
    {
        _api = api;
        ReflectionUtil.LoadedSides ??= api.Side;
        ReflectionUtil.LoadedSides |= api.Side;
        if (api is ICoreClientAPI clientApi) GlobalServiceContainer.Register(clientApi);
        if (api is ICoreServerAPI serverApi) GlobalServiceContainer.Register(serverApi);
        
        ServiceContainer.Register(api);
        ServiceContainer.Register(api.World);
        ServiceContainer.Register(api.Logger);
        EnumExtensionUtil.EnumExtensions[typeof(EnumTransitionType)] = new ExtendedTransition();
        AssetCategoryAttribute.Load();
        AutoRegistryAttribute.RegisterAll(api);
        AutoPatcherAttribute.AutoPatch(api); //TODO: some patches should go earlier (in regards to patching the JSON reading process)
        AutoConfigUtil.LoadAll(api);
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