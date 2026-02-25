using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;
using Vintagestory.Common;

namespace InsanityLib.Extensions;

public static class ModLoaderExtensions
{

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "loadedMods")]
    private static extern ref Dictionary<string, ModContainer> GetLoadedMods(ModLoader instance);

    public static ModContainer? GetMod(this IModLoader modLoader, ReadOnlySpan<char> modID)
    {
        GetLoadedMods((ModLoader)modLoader)
            .GetAlternateLookup<ReadOnlySpan<char>>()
            .TryGetValue(modID, out var result);

        return result;
    }

    public static bool IsModEnabled(this IModLoader modLoader, ReadOnlySpan<char> modID) => modLoader.GetMod(modID) is { Enabled: true };
}
