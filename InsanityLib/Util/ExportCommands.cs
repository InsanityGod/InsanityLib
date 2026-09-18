using InsanityLib.Auto.Command;
using InsanityLib.Auto.Command.Argument;
using IntegratedModManager.Config;
using Newtonsoft.Json;
using System;
using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace InsanityLib.Util;

public static class ExportCommands
{
    /// <summary>
    /// Exports the generated IMM config file to Exports/IMM
    /// </summary>
    /// <param name="capi"></param>
    /// <param name="domain">The specific domain (mod) to export configs for</param>
    /// <param name="pretty">whether it should be pretty printed</param>
    [AutoCommand(Path = "InsanityLib/Export")]
    public static TextCommandResult IMM(ICoreClientAPI capi, [CommandParameter(Provider = EParamProvider.ArgumentParser)] string domain, bool pretty = false)
    {
        const string configName = "config/imm.json";
        try
        {
            var location = new AssetLocation(domain, configName);
            var asset = capi.Assets.TryGet(location);
            if(asset is null) return TextCommandResult.Error("domain doesn't have any IMM configs");
            
            FileInfo fileInfo = new(Path.Combine(GamePaths.DataPath, "Exports/IMM", domain, configName));
            GamePaths.EnsurePathExists(fileInfo.Directory!.FullName);
            var obj = asset.ToObject<ImmConfigDescriptor>();
            
            var json = JsonConvert.SerializeObject(obj, pretty ? Formatting.Indented : Formatting.None, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore, //TODO a lot of unnecesary values are still getting serialized (prob because IMM does not annotate them as default values)
                NullValueHandling = NullValueHandling.Ignore
            });

            File.WriteAllText(fileInfo.FullName, json);
            return TextCommandResult.Success($"Exported to {fileInfo.FullName}");
        }
        catch (Exception ex)
        {
            capi.Logger.Error("[insanitylib] export failed: {0}", ex);
            return TextCommandResult.Error("Export Failed, see logs for more information");
        }
    }
}
