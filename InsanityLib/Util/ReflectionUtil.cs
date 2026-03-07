using HarmonyLib;
using InsanityLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace InsanityLib.Util;

public static class ReflectionUtil
{
    public static EnumAppSide LoadedSides { get; internal set; }

    public static bool SideLoaded(EnumAppSide side) => (LoadedSides & side) != 0;

    public static ICoreAPI GetApi(bool prioritizeServer = true)
    {
        ICoreAPI? result = prioritizeServer ? InsanityLibModSystem.GlobalServiceContainer.GetService<ICoreServerAPI>() : InsanityLibModSystem.GlobalServiceContainer.GetService<ICoreClientAPI>();
        result ??= prioritizeServer ? InsanityLibModSystem.GlobalServiceContainer.GetService<ICoreClientAPI>() : InsanityLibModSystem.GlobalServiceContainer.GetService<ICoreServerAPI>();
        return result!;
    }

    public static IEnumerable<MemberInfo> FindAllMembersHavingAtribute<T>(BindingFlags? flags = null) where T : Attribute => AccessTools.AllTypes()
        .SelectMany(type => type.FindAllMembersHavingAtribute<T>(flags));

    public static IEnumerable<(MemberInfo, T)> FindAllMembersWithAttributes<T>(BindingFlags? flags = null) where T : Attribute => AccessTools.AllTypes()
        .SelectMany(type => type.FindAllMembersWithAttributes<T>(flags));

    public static IEnumerable<(Type, T)> FindAllClasses<T>() where T : Attribute => AccessTools.AllTypes()
        .Select(type => (type, type.TryGetCustomAttribute<T>()))
        .Where(pair => pair.Item2 is not null)!;
}
