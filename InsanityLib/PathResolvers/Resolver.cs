using InsanityLib.PathResolvers.Implementations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;

namespace InsanityLib.PathResolvers;
//TODO allow for operations such as ??, ||, &&, etc
public static class Resolver
{
    public static List<IPathResolver> Resolvers { get; } =
    [
        new WorldPropertiesResolver(),
        new AutoConfigResolver(),
        new ConfigLibResolver(),
        new ModResolver(),
    ];

    public static IPathResolver? Find(ReadOnlySpan<char> scheme)
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
        if (schemeEndIndex == -1) return [];
        
        return pathWithScheme[..schemeEndIndex];
    }

    public static bool TryResolve(ReadOnlySpan<char> pathWithScheme, ICoreAPI api, out object? result)
    {
        var scheme = FindScheme(pathWithScheme);
        if (scheme.IsEmpty)
        {
            result = null;
            return false;
        }
        
        return TryResolve(scheme, pathWithScheme[(scheme.Length + 3)..], api, out result);
    }

    public static bool TryResolve(ReadOnlySpan<char> scheme, ReadOnlySpan<char> path, ICoreAPI api, out object? result)
    {
        var resolver = Find(scheme);
        if(resolver is not null) return resolver.TryResolvePath(path, api, out result);

        result = null;
        return false;
    }

    public static void ResolveAll(ICoreAPI api, JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Object:
            case JTokenType.Array:
            case JTokenType.Property:

                foreach (var child in token.Children().ToArray())
                {
                    ResolveAll(api, child);
                }
                break;

            case JTokenType.String:
                if(token.Parent is not null && TryResolve(token.Value<string>(), api, out var result))
                {
                    var resolvedToken = result is null ? JValue.CreateNull() : JToken.FromObject(result);
                    token.Replace(resolvedToken);
                }
                break;
        }
    }
}
