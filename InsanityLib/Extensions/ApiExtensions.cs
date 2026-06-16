using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace InsanityLib.Extensions;

public static class ApiExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "ServerMods")]
    public static extern ref List<ModId> GetServerMods(this ClientMain clientMain);

    public static bool IsUniversalModPresentOnServer(this ICoreClientAPI capi, string modID) => ((ClientMain)capi.World).GetServerMods().Any(mod => mod.Id == modID);
}
