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
            new CodeMatch(OpCodes.Ldc_I4_0),
            CodeMatch.IsStloc(),
            new CodeMatch(),
            CodeMatch.IsLdloc(),
            CodeMatch.IsLdloc(),
            new CodeMatch(OpCodes.Ldelem_Ref)
        );
        matcher.Advance(1);
        var patchIndex = (LocalBuilder)matcher.Instruction.operand;

        matcher.MatchEndForward(
            new CodeMatch(OpCodes.Ldelem_Ref),
            CodeMatch.IsStloc()
        );
        var patch = (LocalBuilder)matcher.Instruction.operand;

        matcher.MatchStartForward(
            CodeMatch.LoadsField(AccessTools.Field(typeof(JsonPatch), nameof(JsonPatch.Condition))),
            CodeMatch.Branches()
        );
        matcher.Advance(-1);
        var oldLabels = matcher.Labels;
        matcher.Labels = [];

        matcher.InsertAndAdvance(
            CodeInstruction.LoadLocal(patch.LocalIndex).WithLabels(oldLabels), //JsonPatch patch
            CodeInstruction.LoadLocal(patchIndex.LocalIndex), //int patchIndex
            CodeInstruction.LoadLocal(15), //IAsset asset
            CodeInstruction.LoadArgument(0),
            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ModJsonPatchLoader), "api")), //ICoreAPI api
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchingUtil), nameof(PatchingUtil.PreProcessJsonPatchCondition))),
            new CodeInstruction(OpCodes.Brfalse_S, unmentConditionPath) //If not successful, go to unmet condition path
        );
    
        matcher.MatchStartForward(
            new CodeMatch(instruction => instruction.opcode == OpCodes.Ldloc_S && instruction.operand is LocalBuilder local && local.LocalIndex == 5), //5 = unmetConditionCount
            new CodeMatch(OpCodes.Ldc_I4_1),
            new CodeMatch(OpCodes.Add),
            new CodeMatch(instruction => instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder local && local.LocalIndex == 5)
        );
        matcher.Labels.Add(unmentConditionPath);
    
        return matcher.InstructionEnumeration();
    }
}
