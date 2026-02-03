using HarmonyLib;
using InsanityLib.Util;
using System.Collections.Generic;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.ServerMods.NoObf;

namespace InsanityLib.Extended.HarmonyPatches;

[HarmonyPatch]
public static class ExtendedPatchingPatches
{

    [HarmonyPatch(typeof(ModJsonPatchLoader), nameof(ModJsonPatchLoader.ApplyPatch))]
    [HarmonyPrefix]
    public static bool PatchValuePreProcessor(JsonPatch jsonPatch, int patchIndex, AssetLocation patchSourcefile, ICoreAPI ___api, ref int errorCount)
    {
        if(!PatchingUtil.PreProcessJsonPatchValue(jsonPatch, patchIndex, patchSourcefile, ___api))
        {
            errorCount++;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(ModJsonPatchLoader), nameof(ModJsonPatchLoader.ApplyPatches))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> AddPatchConditionPreProcessor(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);
        matcher.DefineLabel(out var unmentConditionPath);

        matcher.MatchStartForward(
            CodeMatch.LoadsField(AccessTools.Field(typeof(JsonPatch), nameof(JsonPatch.Condition))),
            CodeMatch.Branches()
        );
        matcher.Advance(-1);

        matcher.InsertAndAdvance(
            new CodeInstruction(OpCodes.Ldloc_S, 12), //JsonPatch patch
            new CodeInstruction(OpCodes.Ldloc_S, 11), //int patchIndex
            new CodeInstruction(OpCodes.Ldloc_S, 8), //IAsset asset
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ModJsonPatchLoader), "api")), //ICoreAPI api
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchingUtil), nameof(PatchingUtil.PreProcessJsonPatchCondition))),
            new CodeInstruction(OpCodes.Brfalse_S, unmentConditionPath) //If not successful, go to unmet condition path
        );

        matcher.MatchStartForward(
            new CodeMatch(instruction => instruction.opcode == OpCodes.Ldloc_S && instruction.operand is LocalBuilder local && local.LocalIndex == 4),
            new CodeMatch(OpCodes.Ldc_I4_1),
            new CodeMatch(OpCodes.Add),
            new CodeMatch(instruction => instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder local && local.LocalIndex == 4)
        );
        matcher.Labels.Add(unmentConditionPath);

        return matcher.InstructionEnumeration();
    }
}
