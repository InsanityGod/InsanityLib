using HarmonyLib;
using InsanityLib.Util.Span;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace InsanityLib.Extended.Traits.Patches;

[HarmonyPatch]
public static class TraitRenderingPatches
{

    [HarmonyPatch(typeof(CharacterSystem), "getClassTraitText")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> TranspileCharacterMenu(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);
    
        matcher.DefineLabel(out var continueLabel);
        
        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldloc_3),
            new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(GlobalConstants), nameof(GlobalConstants.DefaultCultureInfo)))
        );
        
        matcher.InsertAfterAndAdvance(
            new CodeInstruction(OpCodes.Ldloc_S, 7), // val local
            CodeInstruction.Call(
                typeof(TraitRenderingPatches),
                nameof(TryAddAttributeStringAlternativeFormat)
            ),
            new CodeInstruction(OpCodes.Brtrue_S, continueLabel),
            new CodeInstruction(OpCodes.Ldloc_3)
        );
    
        matcher.MatchEndForward(
            new CodeMatch(code => code.operand is MethodInfo method && method.DeclaringType == typeof(StringBuilder) && method.Name == "Append"),
            new CodeMatch(OpCodes.Pop)
        );
        matcher.Advance();
        matcher.Labels.Add(continueLabel);
    
        ReplaceLangKeys(matcher);
        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch(typeof(GuiDialogCreateCharacter), "changeClass")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> TranspileClassSelection(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);
    
        matcher.DefineLabel(out var continueLabel);
        
        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldloc_2),
            new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(GlobalConstants), nameof(GlobalConstants.DefaultCultureInfo)))
        );
        
        matcher.InsertAfterAndAdvance(
            new CodeInstruction(OpCodes.Ldloc_S, 6), // val local
            CodeInstruction.Call(
                typeof(TraitRenderingPatches),
                nameof(TryAddAttributeStringAlternativeFormat)
            ),
            new CodeInstruction(OpCodes.Brtrue_S, continueLabel),
            new CodeInstruction(OpCodes.Ldloc_2)
        );
    
        matcher.MatchEndForward(
            new CodeMatch(code => code.operand is MethodInfo method && method.DeclaringType == typeof(StringBuilder) && method.Name == "Append"),
            new CodeMatch(OpCodes.Pop)
        );
        matcher.Advance();
        matcher.Labels.Add(continueLabel);

        ReplaceLangKeys(matcher);
        return matcher.InstructionEnumeration();
    }

    public static void ReplaceLangKeys(CodeMatcher matcher)
    {
        matcher.Start();
        matcher.MatchEndForward(
            new CodeMatch(instruction => instruction.opcode == OpCodes.Ldstr && instruction.operand is string str && str.Contains("trait")),
            CodeMatch.LoadsLocal(),
            CodeMatch.LoadsField(AccessTools.Field(typeof(Trait), nameof(Trait.Code))),
            new CodeMatch(instruction => instruction.operand is MethodInfo method && method.Name == "Concat")
        );

        matcher.Repeat(static match =>
        {
            match.Opcode = OpCodes.Call;
            match.Operand = AccessTools.Method(typeof(TraitRenderingPatches), nameof(TranslateKey));
            match.Advance(-1);
            match.RemoveInstruction();
        });
    }

    public static string TranslateKey(string key, Trait trait)
    {
        var code = AssetLocationSpan.Create(trait.Code, allowNoDomain: true);
        string result;
        if(!code.Domain.IsEmpty)
        {
            result = $"{code.Domain}:{key}{code.Path}";
            if(Lang.HasTranslation(result)) return result;
        }
        
        if(key == "trait-")
        {
            var titleKey = TranslateKey("traittitle-", trait);
            if (Lang.HasTranslation(titleKey))
            {
                return Lang.Get($"insanitylib:trait-wrapper-{trait.Type}", Lang.Get(titleKey));
            }
        }

        return key + trait.Code;
    }

    private static bool TryAddAttributeStringAlternativeFormat(StringBuilder attributes, KeyValuePair<string, double> data)
    {
        var lookup = $"charattribute-{data.Key}";

        var result = Lang.GetUnformatted(lookup);
        if(result == lookup || string.IsNullOrEmpty(result)) return false;

        attributes.Append(string.Format(result, data.Value));

        return true;
    }
}
