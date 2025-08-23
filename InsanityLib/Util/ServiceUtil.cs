using System;
using System.ComponentModel.Design;
using Vintagestory.API.Common;

namespace InsanityLib.Util;

public static class ServiceUtil
{
    public static void Register<T>(this IServiceContainer container, T instance) => container.AddService(typeof(T), instance);
    
    public static void Register<T>(this IServiceContainer container) where T : class => container.AddService(typeof(T), container.AutoCreate<T>());

    public static IServiceContainer GetServiceContainer(this ICoreAPI api) => api.ModLoader.GetModSystem<InsanityLibModSystem>().ServiceContainer;

    public static T GetService<T>(this IServiceProvider provider) where T : class => provider.GetService(typeof(T)) as T;

    public static T GetService<T>(this ICoreAPI api) where T : class => api.GetServiceContainer().GetService<T>();
}
