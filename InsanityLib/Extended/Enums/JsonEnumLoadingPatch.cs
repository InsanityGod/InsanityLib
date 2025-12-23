using HarmonyLib;
using InsanityLib.Util;
using System;

namespace InsanityLib.Extended.Enums;

[HarmonyPatch]
internal static class JsonEnumLoadingPatch
{

    [HarmonyPatch("Newtonsoft.Json.Utilities.EnumUtils", "ParseEnum")]
    [HarmonyPrefix]
    internal static bool PrefixExtendedEnum(Type enumType, string value, ref object __result)
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
