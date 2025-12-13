using InsanityLib.Constants;
using InsanityLib.PathResolvers;
using Newtonsoft.Json.Linq;
using System;
using Vintagestory.API.Common;
using Vintagestory.ServerMods.NoObf;

namespace InsanityLib.Util;

internal static class PatchingUtil
{
    private static JToken AsJToken(this object obj) => obj is JToken jToken ? jToken : JToken.FromObject(obj);

    internal static bool PreProcessJsonPatchValue(JsonPatch patch, int patchIndex, AssetLocation patchSourceFile, ICoreAPI api)
    {
        if (patch?.Value?.Token is not JValue value || value.Type != JTokenType.String) return true;
        ReadOnlySpan<char> path = value.Value as string;

        var scheme = Resolver.FindScheme(path);
        if(scheme.IsEmpty) return true;

        var resolver = Resolver.Find(scheme);
        if(resolver is null)
        {
            api.Logger.Error(
                Logging.PatchPathResolverFailed,
                patchIndex,
                patchSourceFile,
                patch.Value,
                $"No resolver found for scheme '{scheme}'"
            );

            return false;
        }

        if (resolver.TryResolvePath(path[(scheme.Length + 3)..], api, out var result))
        {
            patch.Value.Token = result.AsJToken();
            return true;
        }

        api.Logger.Error(
            Logging.PatchPathResolverFailed,
            patchIndex,
            patchSourceFile,
            patch.Value,
            $"Failed to resolve path '{path}'"
        );
        return false;
    }

    internal static bool PreProcessJsonPatchCondition(JsonPatch patch, int patchIndex, IAsset asset, ICoreAPI api)
    {
        if (patch.Condition is null) return true;
        
        ReadOnlySpan<char> path = patch.Condition.When; //TODO maybe allow for resolving both When and IsValue?
        var scheme = Resolver.FindScheme(path);
        
        if(scheme.IsEmpty) return true;

        var resolver = Resolver.Find(scheme);
        if(resolver is null)
        {
            api.Logger.Error(
                Logging.PatchPathResolverFailed,
                patchIndex,
                asset.Location,
                patch.Condition.When,
                $"No resolver found for scheme '{scheme}'"
            );

            return false;
        }

        if (resolver.TryResolvePath(path[(scheme.Length + 3)..], api, out var result))
        {
            //TODO maybe allow for a truthy/falsy comparison?
            var value = result.AsJToken().ToString();

            if(string.Equals(value, patch.Condition.IsValue, StringComparison.InvariantCultureIgnoreCase))
            {
                patch.Condition = null; //Condition is matched and can be ignored
                return true;
            }

            api.Logger.VerboseDebug(
                Logging.PatchUnmentCondition,
                patchIndex,
                asset.Location,
                patch.Condition.When,
                value,
                patch.Condition.IsValue
            );

            return false;
        }

        api.Logger.Error(
            Logging.PatchPathResolverFailed,
            patchIndex,
            asset.Location,
            patch.Value,
            $"Failed to resolve path '{path}'"
        );

        return false;
    }
}
