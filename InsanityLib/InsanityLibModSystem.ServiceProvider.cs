using InsanityLib.Auto.Cleanup;
using InsanityLib.Extensions;
using System;
using System.ComponentModel.Design;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace InsanityLib;

public partial class InsanityLibModSystem : ModSystem, IServiceProvider
{
    [AutoDefaultValue(AutoType = typeof(ServiceContainer))]
    public static IServiceContainer GlobalServiceContainer { get; set; } = new ServiceContainer();
    
    public object? GetService(Type serviceType)
    {
        var result = ServiceContainer?.GetService(serviceType);
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

    private static void RegisterApiServices(ICoreAPI api)
    {
        if (api is ICoreClientAPI clientApi) GlobalServiceContainer.AddService(clientApi);
        if (api is ICoreServerAPI serverApi) GlobalServiceContainer.AddService(serverApi);
    }
}