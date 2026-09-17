using InsanityLib.Extensions;
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
    //TODO maybe some way to convert types
    public static readonly char[] Modifiers = ['!', '-'];

    public static List<IPathResolver> Resolvers { get; } =
    [
        new WorldPropertiesResolver(),
        new AutoConfigResolver(),
        new ConfigLibResolver(), //TODO make work with ConfigKit
        new ModResolver(),
        //TODO IMM resolver maybe?
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
        if (path.IsEmpty)
        {
            result = null;
            return false;
        }

        var resolver = Find(scheme);
        if (resolver is null)
        {
            result = null;
            return false;
        }

        char? modifier = null;
        if (Modifiers.Contains(path[0]))
        {
            modifier = path[0];
            path = path[1..];
        }

        if (resolver.TryResolvePath(path, api, out result))
        {
            if(modifier is null) return true;
            try
            {
                result = ApplyModifier(modifier.Value, result);
                return true;
            }
            catch (Exception ex)
            {
                api.Logger.Error($"[insanitylib] Failed to resolve path '{scheme}://{modifier}{path}', exception: {ex}");
            }
        }
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

    //TODO documentation
    private static object? ApplyModifier(char modifier, object? value) => modifier switch
    {
        '!' => value.IsFalsy(),
        //TODO IsTruthy?
        '-' => Negate(value),
        _ => throw new InvalidOperationException($"Unknown modifier '{modifier}'")
    };

    private static object? Negate(object? value)
    {
        if(value is null) return value;
        try
        {
            return -(dynamic)value!;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Modifier '-' is not valid for {value.GetType()}", ex);
        }
    }
}
