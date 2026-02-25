using InsanityLib.Auto.Config;
using InsanityLib.Constants;
using InsanityLib.Extensions;
using System;
using Vintagestory.API.Common;

namespace InsanityLib.PathResolvers.Implementations;
#pragma warning disable CS8624 // Argument cannot be used as an output for parameter due to differences in the nullability of reference types.

public class AutoConfigResolver : IPathResolver
{
    public string Scheme => "config";

    public bool TryResolvePath(ReadOnlySpan<char> path, ICoreAPI api, out object? result)
    {
        foreach ((ReadOnlySpan<char> configPath, var config) in AutoConfig.Loaded)
        {
            var configName = configPath.WithoutSuffix(".json");
            if (!path.StartsWith(configName)) continue;

            path = path[configName.Length..].WithoutPrefix(".json").WithoutPrefix("/");

            var remainder = config.ConfigInstance.TryCrawl(path, out result);
            if (remainder.IsEmpty) return true;

            object reason;
            if (result is Exception) reason = result;
            else if (result is null) reason = "Null reference in path";
            else reason = "Invalid path";

            api.Logger.Warning(
                Logging.PathResolverFailed,
                nameof(AutoConfigResolver),
                $"{configName}/{path[..^remainder.Length]}",
                remainder.ToString(),
                reason
            );

            return false;
        }

        api.Logger.Warning(
            Logging.PathResolverFailed,
            nameof(AutoConfigResolver),
            string.Empty,
            path.ToString(),
            "Config not found"
        );

        result = null;
        return false;
    }
}
