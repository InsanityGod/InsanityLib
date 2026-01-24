using HarmonyLib;
using InsanityLib.PathResolvers;
using InsanityLib.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace InsanityLib.Extended.Json;

[HarmonyPatch]
internal static class JsonExtensionPatch1
{
    [HarmonyTargetMethods]
    internal static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(JContainer), nameof(JContainer.ReadFrom), [typeof(JsonReader), typeof(JsonLoadSettings)]);
        yield return AccessTools.Method(typeof(JToken), nameof(JToken.ReadFrom), [typeof(JsonReader), typeof(JsonLoadSettings)]);
    }

    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> TranspilePathResolvers(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);

        matcher.MatchStartForward(
            CodeMatch.LoadsArgument(),
            CodeMatch.Calls(AccessTools.PropertyGetter(typeof(JsonReader), nameof(JsonReader.Value))),
            typeof(JValue).CodeMatchConstructor()
        );
        var ldarg = matcher.Instruction.opcode;

        var jump = generator.DefineLabel();
        matcher.InsertAfter(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(JsonExtensionPatch1), nameof(TryPathResolve))),
            new CodeInstruction(OpCodes.Dup),
            new CodeInstruction(OpCodes.Brfalse_S, jump),
            new CodeInstruction(OpCodes.Ret),
            new CodeInstruction(OpCodes.Pop) { labels = [jump] },
            new CodeInstruction(ldarg)
        );

        return matcher.InstructionEnumeration();
    }

    public static JToken? TryPathResolve(JsonReader reader)
    {
        if (reader.TokenType != JsonToken.String || reader.Value is not string stringValue || !Resolver.TryResolve(stringValue, ReflectionUtil.GetApi(), out object? result))
        {
            return null;
        }
        if(result is null) return JValue.CreateNull();

        return JToken.FromObject(result);
    }
}

[HarmonyPatch]
internal static class JsonExtensionPatch2
{
    [HarmonyPatch(typeof(JContainer), "ReadContentFrom", typeof(JsonReader), typeof(JsonLoadSettings))]
    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> TranspilePathResolvers(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);
        matcher.DeclareLocal(typeof(JToken), out var tmpLocal);
        
        matcher.MatchStartForward(
            CodeMatch.LoadsArgument(),
            CodeMatch.Calls(AccessTools.PropertyGetter(typeof(JsonReader), nameof(JsonReader.Value))),
            typeof(JValue).CodeMatchConstructor()
        );
        var ldarg = matcher.Instruction.opcode;

        var jump = generator.DefineLabel();
        var jump2 = generator.DefineLabel();
        matcher.InsertAfterAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(JsonExtensionPatch1), nameof(JsonExtensionPatch1.TryPathResolve))),
            new CodeInstruction(OpCodes.Dup),
            new CodeInstruction(OpCodes.Brfalse_S, jump),
            CodeInstruction.StoreLocal(tmpLocal.LocalIndex),
            CodeInstruction.LoadLocal(tmpLocal.LocalIndex),
            new CodeInstruction(OpCodes.Ldloc_0),
            new CodeInstruction(OpCodes.Ldarg_2),
            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(JToken), "SetLineInfo", [typeof(IJsonLineInfo), typeof(JsonLoadSettings)])),
            new CodeInstruction(OpCodes.Ldloc_1),
            CodeInstruction.LoadLocal(tmpLocal.LocalIndex),
            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(JContainer), nameof(JContainer.Add))),
            new CodeInstruction(OpCodes.Br, jump2),
            new CodeInstruction(OpCodes.Pop) { labels = [jump] },
            new CodeInstruction(ldarg)
        );

        matcher.MatchStartForward(CodeMatch.Branches());
        matcher.Labels.Add(jump2);
        
        return matcher.InstructionEnumeration();
    }
}
