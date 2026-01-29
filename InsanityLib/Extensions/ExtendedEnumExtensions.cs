using InsanityLib.Extended.Enums;
using System;
using System.Collections.Generic;

namespace InsanityLib.Extensions;

public static class ExtendedEnumExtensions
{
    /// <summary>
    /// Registers TExtension as an extension of TActual.<br/>
    /// This allowes one to 
    /// </summary>
    /// <typeparam name="TActual"></typeparam>
    /// <typeparam name="TExtension"></typeparam>
    public static void RegisterEnumExtension<TActual, TExtension>() where TActual : Enum where TExtension : Enum
    {
        if (!ExtendedEnum.EnumExtensions.TryGetValue(typeof(TActual), out var extendedEnum))
        {
            extendedEnum = new ExtendedEnum(typeof(TActual));
            ExtendedEnum.EnumExtensions[typeof(TActual)] = extendedEnum;
        }

        extendedEnum.RegisterExtension<TExtension>();
    }

    public static TExtension ToExtensionEnum<TActual, TExtension>(this TActual actual) where TActual : Enum where TExtension : Enum
    {
        if (ExtendedEnum.EnumExtensions.TryGetValue(typeof(TActual), out var extendedEnum))
        {
            return extendedEnum.FromExtendedEnum<TExtension>((int)(object)actual);
        }
        throw new InvalidOperationException($"{typeof(TActual).FullName} is not an extended enum");
    }

    public static TActual FromExtensionEnum<TExtension, TActual>(this TExtension extension) where TActual : Enum where TExtension : Enum
    {
        if (ExtendedEnum.EnumExtensions.TryGetValue(typeof(TActual), out var extendedEnum))
        {
            return (TActual)(object)extendedEnum.ToExtendedEnum(extension);
        }
        throw new InvalidOperationException($"{typeof(TActual).FullName} is not an extended enum");
    }

    public static int? TryParse(Type type, string strValue) => ExtendedEnum.EnumExtensions.GetValueOrDefault(type)?.FromString(strValue);

    public static string? TryToString(Type type, int intValue) => ExtendedEnum.EnumExtensions.GetValueOrDefault(type)?.ToString(intValue);
}
