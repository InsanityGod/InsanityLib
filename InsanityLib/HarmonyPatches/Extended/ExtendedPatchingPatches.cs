using HarmonyLib;
using InsanityLib.Exceptions;
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
        public static IEnumerable<CodeInstruction> AddPatchValuePreProcessor(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();
            var normalPath = generator.DefineLabel(); // Define a label for the IL generator

            for (var i = 0; i < codes.Count; i++)
            {
                if(i == codes.Count) throw new HarmonyInjectionException("[InsanityLib] Failed to inject JSON Patch value PreProcessor");
                if(codes[i].opcode == OpCodes.Switch)
                {
                    codes[i - 1].labels.Add(normalPath); // Add the label to the previous instruction
                    codes.InsertRange(i - 1, new CodeInstruction[]
                    {
                        new(OpCodes.Ldarg_3), //JsonPatch patch
                        new(OpCodes.Ldarg_1), //int patchIndex
                        new(OpCodes.Ldarg_2), //AssetLocation patchSourceFile
                        new(OpCodes.Ldarg_0),
                        new(OpCodes.Ldfld, AccessTools.Field(typeof(ModJsonPatchLoader), "api")), //ICoreAPI api
                        new(OpCodes.Call, AccessTools.Method(typeof(ConfigUtil), nameof(ConfigUtil.PreProcessJsonPatchValue))),
                        new(OpCodes.Brtrue_S, normalPath), //If successful, continue with the normal path
                        
                        //errorCount++
                        new(OpCodes.Ldarg_S, 6),
                        new(OpCodes.Ldarg_S, 6),
                        new(OpCodes.Ldind_I4),
                        new(OpCodes.Ldc_I4_1),
                        new(OpCodes.Add),
                        new(OpCodes.Stind_I4),

                        new(OpCodes.Ret) // Return early if the patch value could not be processed
                    });
                    break;
                }
            }

            return codes;
        }

        [HarmonyPatch(typeof(ModJsonPatchLoader), nameof(ModJsonPatchLoader.ApplyPatches))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddPatchConditionPreProcessor(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();
            var unmentConditionPath = generator.DefineLabel();

            var fieldToFind = AccessTools.Field(typeof(JsonPatch), nameof(JsonPatch.Condition));

            for(var i = 0; i < codes.Count; i++)
            {
                if(i == codes.Count) throw new HarmonyInjectionException("[InsanityLib] Failed to inject JSON Patch condition PreProcessor");
                var code = codes[i];

                if (code.opcode != OpCodes.Ldfld || code.operand is not FieldInfo field || field != fieldToFind || codes[i + 1].opcode != OpCodes.Brfalse) continue;
                
                for(var j = i; j < codes.Count; j++) //Find where unmet condition is handled and add label
                {
                    if (codes[j].opcode != OpCodes.Br) continue;
                    var code2 = codes[j - 1];
                    if(code2.opcode == OpCodes.Stloc_S && code2.operand is LocalBuilder local && local.LocalIndex == 4)
                    {
                        codes[j - 4].labels.Add(unmentConditionPath);
                        break;
                    }
                }

                codes.InsertRange(i - 1, new CodeInstruction[]
                {
                        new(OpCodes.Ldloc_S, 12), //JsonPatch patch
                        new(OpCodes.Ldloc_S, 11), //int patchIndex
                        new(OpCodes.Ldloc_S, 8), //IAsset asset
                        new(OpCodes.Ldarg_0),
                        new(OpCodes.Ldfld, AccessTools.Field(typeof(ModJsonPatchLoader), "api")), //ICoreAPI api
                        new(OpCodes.Call, AccessTools.Method(typeof(ConfigUtil), nameof(ConfigUtil.PreProcessJsonPatchCondition))),
                        new(OpCodes.Brfalse_S, unmentConditionPath), //If not successful, go to unmet condition path
                });
                break;
            }

            return codes;
        }
    }
}
