using HarmonyLib;
using Newtonsoft.Json;
using System.Text;
using Vintagestory.Server;

namespace InsanityLib.Auto.Config;

/// <summary>
/// Patch to temporarily append config to WorldConfig when sending to client
/// </summary>
[HarmonyPatch(typeof(ServerMain), "WorldMetaDataPacket")]
public static class AppendToWorldConfigPacketPatch
{
    [HarmonyPrefix]
    public static void Prefix(ServerMain __instance)
    {
        var configTree = __instance.World.Config.GetOrAddTreeAttribute("insanitylib_configs");

        foreach((var path, var config) in AutoConfig.Loaded)
        {
            configTree.SetBytes(path,  Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(config, Formatting.None)));
        }
    }

    [HarmonyPostfix]
    public static void Postfix(ServerMain __instance)
    {
        __instance.World.Config.RemoveAttribute("insanitylib_configs");
    }
}

