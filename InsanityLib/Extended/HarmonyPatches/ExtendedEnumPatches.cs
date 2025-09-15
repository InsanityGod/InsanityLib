using HarmonyLib;
using InsanityLib.Util;
using System;

namespace InsanityLib.Extended.HarmonyPatches;

[HarmonyPatch]
public static class ExtendedEnumPatches
{

    [HarmonyPatch("Newtonsoft.Json.Utilities.EnumUtils", "ParseEnum")]
    [HarmonyPrefix]
    public static bool ExtendedEnumParsingPrefix(Type enumType, string value, ref object __result)
    {
        var result = EnumExtensionUtil.TryParse(enumType, value);
        if(result is not null)
        {
            __result = result.Value;
            return false; //Prevent default execution
        }

        return true;
    }
}
