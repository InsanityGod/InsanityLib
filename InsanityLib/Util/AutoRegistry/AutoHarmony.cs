using HarmonyLib;
using InsanityLib.Attributes.Auto;
using InsanityLib.Attributes.Auto.Harmony;
using InsanityLib.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace InsanityLib.Util.AutoRegistry
{
    public static class AutoHarmony
    {
        public static void AutoPatch(this ICoreAPI api)
        {
            var logger = api.GetService<ILogger>();
            
            AutoPatcherAttribute attr = null;
            foreach (var assembly in AccessTools.AllAssemblies())
            {
                try
                {
                    attr = assembly.GetCustomAttribute<AutoPatcherAttribute>();
                    if (attr == null || Harmony.HasAnyPatches(attr.HarmonyId)) continue;

                    var harmony = new Harmony(attr.HarmonyId);
                    harmony.PatchAllUncategorized(assembly);

                    foreach (var mod in api.ModLoader.Mods)
                    {
                        try
                        {
                            harmony.PatchCategory(assembly, mod.Info.ModID);
                        }
                        catch (Exception ex)
                        {
                            logger?.Error(Logging.ExecutionFailedTemplate, $"{nameof(AutoPatch)} compatibility", $"{attr.HarmonyId} {mod.Info.ModID}", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger?.Error(Logging.ExecutionFailedTemplate, nameof(AutoPatch), attr != null ? attr.HarmonyId : assembly, ex);
                }
            }
        }


        [DisposalLogic]
        public static void AutoHarmonyDisposal(ICoreAPI api)
        {
            var logger = api.GetService<ILogger>();
            AutoPatcherAttribute attr = null;
            foreach (var assembly in AccessTools.AllAssemblies())
            {
                try
                {
                    attr = assembly.GetCustomAttribute<AutoPatcherAttribute>();
                    if(attr != null)
                    {
                        var harmony = new Harmony(attr.HarmonyId);
                        harmony.UnpatchAll(attr.HarmonyId);
                    }
                }
                catch (Exception ex)
                {
                    logger?.Error(Logging.ExecutionFailedTemplate, nameof(AutoHarmonyDisposal), attr != null ? attr.HarmonyId : assembly, ex);
                }
            }
        }
    }
}
