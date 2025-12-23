using InsanityLib.Auto.Cleanup;
using InsanityLib.Extended.Enums;
using System;
using System.Collections.Generic;

namespace InsanityLib.Util;

public static class EnumExtensionUtil
{
    [AutoClear]
    internal static Dictionary<Type, ExtendedEnum> EnumExtensions = [];

    public static void RegisterEnumExtension<TActual, TExtension>() where TActual : Enum where TExtension : Enum
    {
        if (!EnumExtensions.TryGetValue(typeof(TActual), out var extendedEnum))
        {
            extendedEnum = new ExtendedEnum(typeof(TActual));
            EnumExtensions[typeof(TActual)] = extendedEnum;
        }

        extendedEnum.RegisterExtension<TExtension>();
    }

    public static TExtension ToExtensionEnum<TActual, TExtension>(this TActual actual) where TActual : Enum where TExtension : Enum
    {
        if (EnumExtensions.TryGetValue(typeof(TActual), out var extendedEnum))
        {
            return extendedEnum.FromExtendedEnum<TExtension>((int)(object)actual);
        }
        throw new InvalidOperationException($"{typeof(TActual).FullName} is not an extended enum");
    }

    public static TActual FromExtensionEnum<TExtension, TActual>(this TExtension extension) where TActual : Enum where TExtension : Enum
    {
        if (EnumExtensions.TryGetValue(typeof(TActual), out var extendedEnum))
        {
            return (TActual)(object)extendedEnum.ToExtendedEnum(extension);
        }
        throw new InvalidOperationException($"{typeof(TActual).FullName} is not an extended enum");
    }

    public static int? TryParse(Type type, string strValue) => EnumExtensions.GetValueOrDefault(type)?.FromString(strValue);
}
