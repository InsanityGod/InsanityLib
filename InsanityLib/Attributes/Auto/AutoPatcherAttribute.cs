using HarmonyLib;
using InsanityLib.Constants;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Attributes.Auto;

[AttributeUsage(AttributeTargets.Assembly)]
public class AutoPatcherAttribute : Attribute
{
    public readonly string HarmonyId;

    public AutoPatcherAttribute(string harmonyId)
    {
        if (string.IsNullOrEmpty(harmonyId)) throw new ArgumentException($"'{nameof(harmonyId)}' cannot be null or empty.", nameof(harmonyId));
        HarmonyId = harmonyId;
    }

    internal static void AutoPatch(ICoreAPI api)
    {
        var logger = api.GetService<ILogger>();

        AutoPatcherAttribute attr = null;
        foreach (var assembly in AccessTools.AllAssemblies())
        {
            try
            {
                attr = assembly.GetCustomAttribute<AutoPatcherAttribute>();
                if (attr is null || Harmony.HasAnyPatches(attr.HarmonyId)) continue;

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
                logger?.Error(Logging.ExecutionFailedTemplate, nameof(AutoPatch), attr is not null ? attr.HarmonyId : assembly, ex);
            }
        }
    }

    [DisposalLogic]
    private static void AutoHarmonyDisposal(ICoreAPI api)
    {
        var logger = api.GetService<ILogger>();
        AutoPatcherAttribute attr = null;
        foreach (var assembly in AccessTools.AllAssemblies())
        {
            try
            {
                attr = assembly.GetCustomAttribute<AutoPatcherAttribute>();
                if (attr is not null)
                {
                    var harmony = new Harmony(attr.HarmonyId);
                    harmony.UnpatchAll(attr.HarmonyId);
                }
            }
            catch (Exception ex)
            {
                logger?.Error(Logging.ExecutionFailedTemplate, nameof(AutoHarmonyDisposal), attr is not null ? attr.HarmonyId : assembly, ex);
            }
        }
    }
}
