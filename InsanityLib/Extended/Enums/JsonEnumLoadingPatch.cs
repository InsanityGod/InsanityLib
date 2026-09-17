using HarmonyLib;
using InsanityLib.Extensions;
using System;
using System.Collections.Generic;

namespace InsanityLib.Extended.Enums;

[HarmonyPatch]
internal static class JsonEnumLoadingPatch
{

    [HarmonyPatch("Newtonsoft.Json.Utilities.EnumUtils", "ParseEnum")]
    [HarmonyPrefix]
    internal static bool PrefixExtendedEnum(Type enumType, string value, ref object __result)
    {
        if(ExtendedEnum.EnumExtensions.GetValueOrDefault(enumType)?.FromString(value) is { } result)
        {
            __result = result;
            return false; //Prevent default execution
        }

        return true;
    }
}
