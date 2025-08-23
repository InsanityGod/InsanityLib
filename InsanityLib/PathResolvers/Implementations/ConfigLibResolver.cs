using ConfigLib;
using HarmonyLib;
using InsanityLib.Constants;
using InsanityLib.Util.SpanUtil;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace InsanityLib.PathResolvers.Implementations;

public class ConfigLibResolver : IPathResolver
{
    public string Scheme => "configlib";

    public bool TryResolvePath(ReadOnlySpan<char> path, ICoreAPI api, out object result)
    {
        if(!api.ModLoader.IsModEnabled("configlib"))
        {
            api.Logger.Warning(Logging.ModRequirementNotMet, nameof(ConfigLibResolver), "configlib");

            result = null;
            return false;
        }
        return TryResolvePathInternal(path, api, out result);
    }

    private bool TryResolvePathInternal(ReadOnlySpan<char> path, ICoreAPI api, out object result)
    {
        var configLib = api.ModLoader.GetModSystem<ConfigLibModSystem>();
        var configs = Traverse.Create(configLib).Field<Dictionary<string, ConfigLib.Config>>("_configs").Value;

        foreach((ReadOnlySpan<char> configPath, var config) in configs)
        {
            var configName = configPath.WithoutSuffix(".json");
            if (!path.StartsWith(configName)) continue;

            var settingPath = path[configName.Length..].WithoutPrefix(".json").WithoutPrefix("/").ToString();

            var setting = config.GetSetting(settingPath);

            if (setting is not null)
            {
                result = setting.Value.Token;
                return true;
            }
            else result = null;

            api.Logger.Warning(
                Logging.PathResolverFailed,
                nameof(ConfigLibResolver),
                path[..^settingPath.Length].ToString(),
                settingPath,
                "Setting not found"
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
