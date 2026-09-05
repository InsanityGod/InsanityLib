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
    [HarmonyPrepare]
    public static bool Prepare() => TargetMethod() is not null;

    [HarmonyTargetMethod]
    public static MethodBase TargetMethod() => AccessTools.Method("XLib.XLeveling.Ability:FormattedDescription");

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
