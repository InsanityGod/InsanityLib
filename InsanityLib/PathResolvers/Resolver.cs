using InsanityLib.PathResolvers.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;

namespace InsanityLib.PathResolvers;

public static class Resolver
{
    //TODO allow for using these in more the just JSON patches
    public static List<IPathResolver> Resolvers { get; } =
    [
        new WorldPropertiesResolver(),
        new AutoConfigResolver(),
        new ConfigLibResolver(),
    ];

    public static IPathResolver Find(ReadOnlySpan<char> scheme)
    {
        foreach (var resolver in Resolvers)
        {
            if (scheme.SequenceEqual(resolver.Scheme)) return resolver;
        }
        return null;
    }

    public static ReadOnlySpan<char> FindScheme(ReadOnlySpan<char> pathWithScheme)
    {
        var schemeEndIndex = pathWithScheme.IndexOf("://", StringComparison.Ordinal);
        if (schemeEndIndex == -1) return ReadOnlySpan<char>.Empty;
        
        return pathWithScheme[..schemeEndIndex];
    }

    public static bool TryResolve(ReadOnlySpan<char> pathWithScheme, ICoreAPI api, out object result)
    {
        var scheme = FindScheme(pathWithScheme);
        if (scheme.IsEmpty)
        {
            result = null;
            return false;
        }
        
        return TryResolve(scheme, pathWithScheme[(scheme.Length + 3)..], api, out result);
    }

    public static bool TryResolve(ReadOnlySpan<char> scheme, ReadOnlySpan<char> path, ICoreAPI api, out object result)
    {
        var resolver = Find(scheme);
        if(resolver is not null) return resolver.TryResolvePath(path, api, out result);

        result = null;
        return false;
    }
}
