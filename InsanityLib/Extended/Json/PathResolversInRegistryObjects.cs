using HarmonyLib;
using InsanityLib.PathResolvers;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;
using Vintagestory.ServerMods.NoObf;

namespace InsanityLib.Extended.Json;

[HarmonyPatch]
internal static class PathResolversInRegistryObjects
{
    [HarmonyPatch(typeof(RegistryObjectType), "CreateBasetype")]
    [HarmonyPrefix]
    internal static void PreProcessPathResolvers(ICoreAPI api, JObject entityTypeObject) => Resolver.ResolveAll(api, entityTypeObject);
}
