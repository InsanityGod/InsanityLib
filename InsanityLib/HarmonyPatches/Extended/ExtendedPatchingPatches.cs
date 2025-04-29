using HarmonyLib;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vintagestory.ServerMods.NoObf;

namespace InsanityLib.HarmonyPatches.Extended
{
    [HarmonyPatch]
    public static class ExtendedPatchingPatches
    {
        [HarmonyPatch(typeof(ModJsonPatchLoader), nameof(ModJsonPatchLoader.ApplyPatch))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddPatchValuePreProcessor(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

            for(var i = 0; i < codes.Count; i++)
            {
                if(i == codes.Count) throw new Exception($"Failed to inject patch {nameof(ConfigUtil.PreProcessJsonPatchValue)}");
                if(codes[i].opcode == OpCodes.Switch)
                {
                    codes.InsertRange(i - 1, new CodeInstruction[]
                    {
                        new(OpCodes.Ldarg_3), //JsonPatch patch
                        new(OpCodes.Ldarg_1), //int patchIndex
                        new(OpCodes.Ldarg_2), //AssetLocation patchSourceFile
                        new(OpCodes.Ldarg_0),
                        new(OpCodes.Ldfld, AccessTools.Field(typeof(ModJsonPatchLoader), "api")), //ICoreAPI api
                        new(OpCodes.Call, AccessTools.Method(typeof(ConfigUtil), nameof(ConfigUtil.PreProcessJsonPatchValue)))
                    });
                    break;
                }
            }

            return codes;
        }

        [HarmonyPatch(typeof(ModJsonPatchLoader), nameof(ModJsonPatchLoader.ApplyPatches))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddPatchConditionPreProcessor(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var fieldToFind = AccessTools.Field(typeof(JsonPatch), nameof(JsonPatch.Condition));

            for(var i = 0; i < codes.Count; i++)
            {
                if (i == codes.Count) throw new Exception($"Failed to inject {nameof(ConfigUtil.PreProcessJsonPatchCondition)}");
                var code = codes[i];
                if (code.opcode != OpCodes.Ldfld || code.operand is not FieldInfo field || field != fieldToFind) continue;

                codes.InsertRange(i - 1, new CodeInstruction[]
                {
                        new(OpCodes.Ldloc_S, 12), //JsonPatch patch
                        new(OpCodes.Ldloc_S, 11), //int patchIndex
                        new(OpCodes.Ldloc_S, 8), //IAsset asset
                        new(OpCodes.Ldarg_0),
                        new(OpCodes.Ldfld, AccessTools.Field(typeof(ModJsonPatchLoader), "api")), //ICoreAPI api
                        new(OpCodes.Call, AccessTools.Method(typeof(ConfigUtil), nameof(ConfigUtil.PreProcessJsonPatchCondition))),
                });
                break;
            }

            return codes;
        }
    }
}
