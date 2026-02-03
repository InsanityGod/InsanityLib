using System;
using System.ComponentModel.Design;
using Vintagestory.API.Common;

namespace InsanityLib.Extensions;

public static class ServiceExtensions
{
    public static void AddService<T>(this IServiceContainer container, T instance) => container.AddService(typeof(T), instance!);
    
    public static void AddService<T>(this IServiceContainer container) where T : class => container.AddService(typeof(T), (_, _) => container.AutoCreate<T>(returnNullOnFailure: false));
    public static void AddService<TServiceType, TClass>(this IServiceContainer container) where TClass : class, TServiceType => container.AddService(typeof(TServiceType), (_, _) => container.AutoCreate<TClass>(returnNullOnFailure: false));

    public static IServiceContainer GetServiceContainer(this ICoreAPI api) => api.ModLoader.GetModSystem<InsanityLibModSystem>().ServiceContainer!;
    
    public static IServiceProvider GetServiceProvider(this ICoreAPI api) => api.ModLoader.GetModSystem<InsanityLibModSystem>();

    public static T? GetService<T>(this IServiceProvider provider) where T : class => provider.GetService(typeof(T)) as T;

    public static T? GetService<T>(this ICoreAPI api) where T : class => api.GetServiceProvider().GetService<T>();
}
