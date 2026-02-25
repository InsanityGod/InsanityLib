using InsanityLib.Extensions;
using System;
using Vintagestory.API.Common;

namespace InsanityLib.PathResolvers.Implementations;

public class ModResolver : IPathResolver
{
    public string Scheme => "mod";

    public bool TryResolvePath(ReadOnlySpan<char> path, ICoreAPI api, out object? result)
    {
        result = api.ModLoader.IsModEnabled(path);
        return !path.IsEmpty;
    }
}
