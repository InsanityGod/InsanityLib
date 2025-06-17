using InsanityLib.Constants;
using InsanityLib.Util.AutoRegistry;
using Newtonsoft.Json.Linq;
using System;
using Vintagestory.API.Common;
using Vintagestory.ServerMods.NoObf;

namespace InsanityLib.Util
{
    public static class ConfigUtil
    {
        public static JToken Resolve(string uri) => JToken.FromObject(ResolveObj(uri));

        public static object ResolveObj(string uri)
        {
            if (!ConfigPatchPrefix.TryRemoveFrom(ref uri)) throw new InvalidOperationException("Invalid URI scheme");
            foreach((var path, var config) in AutoConfig.LoadedConfigs)
            {
                var prefix = $"{path.RemoveSuffix(".json")}/";
                if(!prefix.TryRemoveFrom(ref uri)) continue;

                if(!config.TryCrawl(uri, out var resolvedObj)) throw new InvalidOperationException("Could not find path");
                return resolvedObj;
            }

            throw new InvalidOperationException($"Could not find config file");
        }

        public const string ConfigPatchPrefix = "config://"; //TODO configlib prefix
        internal static void PreProcessJsonPatchValue(JsonPatch patch, int patchIndex, AssetLocation patchSourceFile, ICoreAPI api)
        {
            if(patch?.Value?.Token?.Type != JTokenType.String) return;

            var uri = patch.Value.AsString();

            if(!uri.StartsWith(ConfigPatchPrefix)) return;
            try
            {
                patch.Value.Token = Resolve(uri); //TODO TEST
            }
            catch(Exception ex)
            {
                api.GetService<ILogger>()?.Error(Logging.PatchConfigValueResolverFailed, patchIndex, patchSourceFile, patch.Value.AsString(), ex);
            }
        }

        internal static void PreProcessJsonPatchCondition(JsonPatch patch, int patchIndex, IAsset asset, ICoreAPI api)
        {
            if(patch.Condition == null || !patch.Condition.When.StartsWith(ConfigPatchPrefix)) return;
            var uri = patch.Condition.When;

            try
            {
                var value = Resolve(uri).ToString();
                if(string.Equals(value, patch.Condition.IsValue, StringComparison.InvariantCultureIgnoreCase))
                {
                    patch.Condition = null; //Condition matched
                }
            }
            catch(Exception ex)
            {
                api.GetService<ILogger>()?.Error(Logging.PatchConfigValueResolverFailed, patchIndex, asset.Location, patch.Value.AsString(), ex);
            }
        }
    }
}
