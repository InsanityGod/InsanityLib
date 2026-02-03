using HarmonyLib;
using System;
using System.ComponentModel;

namespace InsanityLib.Extensions;

public static class ConversionExtensions
{
    public static T? AutoConvert<T>(this object value) => (T?)value.AutoConvert(typeof(T));

    /// <summary>
    /// Convert object into a different type, through the use of:<br/>
    /// <list type="number">
    /// <item>Enum.Parse</item>
    /// <item>Convert.ChangeType</item>
    /// <item>TypeConverter</item>
    /// <item>Casting</item>
    /// </list>
    /// </summary>
    /// <returns>converted object</returns>
    /// <exception cref="InvalidCastException">If conversion failed</exception>
    public static object? AutoConvert(this object? value, Type? targetType)
    {
        if(value is null || targetType is null || targetType.IsInstanceOfType(value)) return value;

        // Handle enums
        if (targetType.IsEnum && value is string str && Enum.TryParse(targetType, str, out var result)) return result;

        // Handle other types
        if(value is IConvertible) //TODO see about improving performance
        {
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch { /* fail silently */ }
        }

        try
        {

            var converter = TypeDescriptor.GetConverter(targetType);
            if (converter.CanConvertFrom(value.GetType())) return converter.ConvertFrom(value);
        }
        catch { /* fail silently */ }
        
        try
        {
            return value.Cast(targetType);
        }
        catch
        {
            var notNullableType = Nullable.GetUnderlyingType(targetType);
            if(notNullableType is not null) return value.AutoConvert(notNullableType);
            else throw;
        }
    }

    private static T CastWrapper<T>(dynamic value) => (T)value;

    /// <summary>
    /// Dynamicly cast to gived type
    /// </summary>
    /// <returns>casted object</returns>
    /// <exception cref="InvalidCastException">When the cast failed</exception>
    public static object? Cast(this object value, Type targetType) => AccessTools.Method(typeof(ConversionExtensions), nameof(CastWrapper))
        .MakeGenericMethod(targetType)
        .Invoke(null, [value]);

    private static T? DefaultWrapper<T>() => default;

    /// <summary>
    /// Get the default value of a type
    /// </summary>
    /// <returns>default value as if you ran default(YourType)</returns>
    public static object? Default(this Type type) => AccessTools.Method(typeof(ConversionExtensions), nameof(DefaultWrapper))
        .MakeGenericMethod(type)
        .Invoke();

    /// <summary>
    /// Returns the value if it is of the target type, otherwise returns the default value
    /// </summary>
    public static object? As(this object value, Type targetType, object? defaultValue = null) => targetType.IsInstanceOfType(value) ? value : defaultValue;

    /// <summary>
    /// Returns the value if it is of the target type, otherwise returns the default value
    /// </summary>
    public static T? As<T>(this object value, Type targetType, object? defaultValue = null) where T : class => (targetType.IsInstanceOfType(value) ? value : defaultValue) as T;
}
