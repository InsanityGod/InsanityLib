using System;
using Vintagestory.API.Common;

namespace InsanityLib.Extensions;

public static class AssetLocationExtensions
{
    public static AssetLocation FillWildCard(this AssetLocation location, ReadOnlySpan<char> filler) => new(location.Domain, location.Path.Replace("*", filler.ToString()));

    public static string ToStringSimple(this AssetLocation location)
    {
        if(string.IsNullOrEmpty(location.Domain)) return location.Path;
        return $"{location.Domain}:{location.Path}";
    }
}
