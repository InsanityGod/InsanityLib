using InsanityLib.Extended.Enums;
using System;
using System.Collections.Generic;

namespace InsanityLib.Extensions;

public static class ExtendedEnumExtensions
{
    /// <summary>
    /// Registers TExtension as an extension of TActual.<br/>
    /// This allowes one to store TExtension inside TActual with an offset.
    /// </summary>
    public static void RegisterEnumExtension<TActual, TExtension>() where TActual : Enum where TExtension : Enum
    {
        if (!ExtendedEnum.EnumExtensions.TryGetValue(typeof(TActual), out var extendedEnum))
        {
            extendedEnum = new ExtendedEnum(typeof(TActual));
            ExtendedEnum.EnumExtensions[typeof(TActual)] = extendedEnum;
        }

        extendedEnum.RegisterExtension<TExtension>();
    }

    /// <summary>
    /// Extracts the TExtension stored inside TActual (by subtracting the offset calculated during registration)
    /// </summary>
    /// <exception cref="InvalidOperationException" />
    public static TExtension ToExtensionEnum<TActual, TExtension>(this TActual actual) where TActual : Enum where TExtension : Enum
    {
        if (ExtendedEnum.EnumExtensions.TryGetValue(typeof(TActual), out var extendedEnum))
        {
            return extendedEnum.FromExtendedEnum<TExtension>((int)(object)actual);
        }
        throw new InvalidOperationException($"{typeof(TActual).FullName} is not an extended enum");
    }

    /// <summary>
    /// Stores TExtension inside TActual (by adding the offset calculated during registration)
    /// </summary>
    /// <exception cref="InvalidOperationException" />
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
