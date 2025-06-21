using Cairo;
using InsanityLib.Constants;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using InsanityLib.Util.SpanUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace InsanityLib.PathResolvers.Implementations
{
    public class AutoConfigResolver : IPathResolver
    {
        public string Scheme => "config";

        public bool TryResolvePath(ReadOnlySpan<char> path, ICoreAPI api, out object result)
        {
            foreach((ReadOnlySpan<char> configPath, var config) in AutoConfig.LoadedConfigs) //Sadly can't use AlternativeLookup yet
            {
                var configName = configPath.WithoutSuffix(".json");
                if (!path.StartsWith(configName)) continue;

                path = path[configName.Length..].WithoutPrefix(".json").WithoutPrefix("/");

                var remainder = config.ConfigInstance.TryCrawl2(path, out result);
                if (remainder.IsEmpty) return true;

                object reason;
                if (result is Exception) reason = result;
                else if (result is null) reason = "Null reference in path";
                else reason = "Invalid path";

                api.Logger.Warning(
                    Logging.PathResolverFailed,
                    nameof(AutoConfigResolver),
                    path[..^remainder.Length].ToString(),
                    remainder.ToString(),
                    reason
                );

                return false;
            }

            api.Logger.Warning(
                Logging.PathResolverFailed,
                nameof(AutoConfigResolver),
                path.ToString(),
                string.Empty,
                "Config not found"
            );

            result = null;
            return false;
        }
    }
}
