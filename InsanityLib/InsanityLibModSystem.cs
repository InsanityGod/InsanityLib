using InsanityLib.Auto.Command;
using InsanityLib.Documentation;
using InsanityLib.Extended.AssetCategories;
using InsanityLib.Extended.Transitions;
using InsanityLib.Extensions;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using InsanityLib.Util.ContentFeatures;
using System;
using System.ComponentModel.Design;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace InsanityLib;

public partial class InsanityLibModSystem : ModSystem, IServiceProvider
{
    public static IServiceContainer GlobalServiceContainer { get; set; } = new ServiceContainer();

    public object? GetService(Type serviceType)
    {
        var result = ServiceContainer.GetService(serviceType);
        if (result is not null) return result;

        if(_api is null) return null;

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
        if (api is ICoreClientAPI clientApi) GlobalServiceContainer.AddService(clientApi);
        if (api is ICoreServerAPI serverApi) GlobalServiceContainer.AddService(serverApi);

        ReflectionUtil.LoadedSides ??= api.Side;
        ReflectionUtil.LoadedSides |= api.Side;

        AutoSetup(api);
        
        EnumExtensionUtil.EnumExtensions[typeof(EnumTransitionType)] = new ExtendedTransition();
        AssetCategoryAttribute.Load();

        AutoConfigUtil.LoadAll(api);
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        CustomTransition.LoadAssets(api);
    }

    public override void Start(ICoreAPI api)
    {
        AutoCommandAttribute.FindAndRegisterAutoCommands(api);

        //Clear documentation cache build up by auto registration code
        AssemblyDocumentationContext.ClearCache();
    }

    public override void Dispose()
    {
        AutoDispose();
        if(ServiceContainer is null || _api is null) return;
        
        if(ReflectionUtil.LoadedSides is not null)
        {
            ReflectionUtil.LoadedSides &= ~_api.Side;
        }
    }
}