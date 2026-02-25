using InsanityLib.Generators.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InsanityLib.Extended.Enums;

//TODO maybe rework this to allow for a "domain"
//TODO support for flags
public class ExtendedEnum
{
    [AutoClear]
    internal readonly static Dictionary<Type, ExtendedEnum> EnumExtensions = [];

    public Type EnumType { get; }

    internal readonly OrderedDictionary<object, int> OffsetLookup = [];
    protected int currentOffset;

    public readonly int defaultMaxValue;

    public ExtendedEnum(Type enumType)
    {
        if(enumType.GetCustomAttribute<FlagsAttribute>() is not null) throw new InvalidOperationException($"Cannot extend {enumType}: Enum extension is not supported for flags");
        EnumType = enumType;
        defaultMaxValue = Enum.GetValues(enumType).Cast<int>().Max();
        currentOffset = defaultMaxValue + 1;

    }

    public void RegisterExtension<T>() where T : Enum
    {
        var enumType = typeof(T);
        if (OffsetLookup.ContainsKey(enumType)) return;
        if(enumType.GetCustomAttribute<FlagsAttribute>() is not null) throw new InvalidOperationException($"Cannot extend {enumType}: Enum extension is not supported for flags");
        
        var offset = Enum.GetValues(enumType).Cast<int>().Max() + 1;
        OffsetLookup[enumType] = currentOffset;
        currentOffset += offset;
    }

    /// <summary>
    /// Shifts the value of the ExtendedEnum to the (registered) target Enum
    /// </summary>
    /// <typeparam name="T">The target enum</typeparam>
    /// <param name="val">The value of the ExtendedEnum</param>
    public T FromExtendedEnum<T>(int val) where T : Enum => (T)Enum.ToObject(typeof(T), val - OffsetLookup[typeof(T)]);
    
    /// <summary>
    /// Shifts the value of the (registered) Enum to the ExtendedEnum
    /// </summary>
    /// <typeparam name="T">The enum</typeparam>
    /// <param name="val">The value of the Enum</param>
    public int ToExtendedEnum<T>(T val) where T : Enum => (int)Enum.ToObject(typeof(T), val) + OffsetLookup[typeof(T)];

    public virtual int? FromString(string value)
    {
        foreach ((var obj, var offset) in OffsetLookup) 
        {
            if(obj is string strObj && string.Equals(strObj, value, StringComparison.OrdinalIgnoreCase)) return offset;
            
            if(obj is not Type type) continue;
            var name =  Array.Find(Enum.GetNames(type), name => string.Equals(name, value, StringComparison.OrdinalIgnoreCase));
            if(name is null) continue;

            return (int)Enum.Parse(type, name) + offset;
        }

        return null;
    }

    public virtual string? ToString(int value)
    {
        if (currentOffset < 2 || defaultMaxValue >= value) return Enum.ToObject(EnumType, value).ToString();

        var extendedMapping = new EnumNameValueMapping(EnumType);

        return extendedMapping.GetStringValue(value);
    }
}
