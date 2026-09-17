using HarmonyLib;
using InsanityLib.Util;
using InsanityLib.Util.Span;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Vintagestory.API.Config;
using XLib.XLeveling;

namespace InsanityLib.Extended.Traits.Compatibility.XLib.Patches;

[HarmonyPatchCategory("feature:extendedtraits_xlib")]
[HarmonyPatch]
internal static class XLibTraitsLanguagePatch
{

    [HarmonyPatch("XLib.XLeveling.TraitRequirement", "ShortDescription")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> TranspileCharacterMenu(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);
        
        matcher.MatchEndForward(
            new CodeMatch(instruction => instruction.opcode == OpCodes.Ldstr && instruction.operand is string str && str.Contains("trait")),
            CodeMatch.LoadsLocal(),
            new CodeMatch(instruction => instruction.operand is MethodInfo method && method.Name == "Concat")
        );

        matcher.Opcode = OpCodes.Call;
        matcher.Operand = AccessTools.Method(typeof(XLibTraitsLanguagePatch), nameof(TranslateKey));
        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch("XLib.XLeveling.Ability", "FormattedDescription")]
    [HarmonyPrefix]
    internal static bool Prefix(object __instance,int tier, ref string __result)
    {
        if(__instance is not StatsAbility ability) return true;

         var insanityLib = ReflectionUtil.GetApi(false).ModLoader.GetModSystem<InsanityLibModSystem>();
        var extendedTrait = insanityLib.GetExtendedTrait(ability.Name);

        if(extendedTrait is null) return true;

        var values = new object[extendedTrait.Attributes.Count];
        for(int i = 0; i < values.Length; i++)
        {
            values[i] = ability.Value(tier, i) / 100d;
        }
        __result = string.Format(ability.Description, args: values);
        return false;
    }

    internal static string TranslateKey(string key, string trait)
    {
        var code = AssetLocationSpan.Create(trait, allowNoDomain: true);
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
                return Lang.Get(titleKey);
            }
        }

        return key + trait;
    }
}
