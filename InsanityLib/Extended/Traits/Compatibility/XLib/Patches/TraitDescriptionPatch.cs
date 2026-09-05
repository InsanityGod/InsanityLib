using HarmonyLib;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using XLib.XLeveling;

namespace InsanityLib.Extended.Traits.Compatibility.XLib.Patches;

[HarmonyPatch]
public static class TraitDescriptionPatch
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        var method = AccessTools.Method("XLib.XLeveling.Ability:FormattedDescription");
        if(method is not null) yield return method;
    }

    [HarmonyPatch]
    [HarmonyPrefix]
    public static bool Prefix(object __instance,int tier, ref string __result)
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
}
