using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Vintagestory.API.Util;

namespace InsanityLib.Extended.Enums;

public class EnumNameValueMapping
{

    //TODO extended enum support

    //TODO make enums in configs load/save as string instead of number
    public EnumNameValueMapping(Type enumType, bool includeExtended = true)
    {
        EnumType = enumType;
        IsEnumFlag = enumType.GetCustomAttribute<FlagsAttribute>() is not null;
        

        StrValues = Enum.GetNames(enumType);
        
        Names = [.. Enum.GetNames(enumType).Select(NamingExtensions.ToHumanReadable)];

        NumericValues = [.. enumType.GetEnumValues()
            .Cast<int>()
            .Select(static x => (long)x)];

        if (includeExtended && EnumExtensionUtil.EnumExtensions.TryGetValue(enumType, out var extension))
        {
            foreach((var value, var offset) in extension.OffsetLookup)
            {
                if(value is string singleRegistry)
                {
                    var code = singleRegistry.ToAssetLocation(); //TODO maybe an option to include domain between brackets
                    
                    StrValues = StrValues.Append(singleRegistry);
                    Names = Names.Append(code.Path.ToHumanReadable());

                    NumericValues = NumericValues.Append(offset);
                    continue;
                }

                if(value is not Type enumExtensionType || !enumExtensionType.IsEnum) continue;

                StrValues = StrValues.Append(
                    Enum.GetNames(enumExtensionType)
                );
                
                Names = Names.Append(
                    Enum.GetNames(enumExtensionType)
                        .Select(NamingExtensions.ToHumanReadable)
                );

                NumericValues = NumericValues.Append(
                    enumExtensionType.GetEnumValues()
                    .Cast<int>()
                    .Select(static x => (long)x)
                );

            }
        }

        //TODO TEST filter out values that represent more then 1 flag
        if (IsEnumFlag)
        {
            var filteredStrValues = new List<string>();
            var filteredNames = new List<string>();
            var filteredNumericValues = new List<long>();

            for (int i = 0; i < NumericValues.Length; i++)
            {
                long val = NumericValues[i];
                // Only include values that are powers of two (single flag) or zero
                if (val == 0 || (val & val - 1) == 0)
                {
                    filteredStrValues.Add(StrValues[i]);
                    filteredNames.Add(Names[i]);
                    filteredNumericValues.Add(val);
                }
            }

            StrValues = [.. filteredStrValues];
            Names = [.. filteredNames];
            NumericValues = [.. filteredNumericValues];
        }
    }

    public string GetStringValue(object value)
    {
        if (!EnumType.IsInstanceOfType(value)) value = value.AutoConvert(EnumType)!;
        return Enum.Format(EnumType, value, "G");
    }

    public string[] GetStringValues(object value) => GetStringValue(value).Split(", ");

    public int[] GetIndexes(long value) => [.. GetStringValues(value).Select(str => Array.IndexOf(StrValues, str))];

    public string GetDescriptionStrings()
    {
        //TODO also allow for displaying description of the enum values itself
        var builder = new StringBuilder();

        for(var i = 0; i < StrValues.Length; i++)
        {
            builder.Append($"{Names[i]} ({NumericValues[i]})");

            if(i != StrValues.Length - 1) builder.Append(", ");
        }

        return builder.ToString();
    }

    public string GetDisplayString(long value)
    {
        if (!IsEnumFlag) return Names[NumericValues.IndexOf(value)];
        var builder = new StringBuilder();

        for(var i = 0; i < NumericValues.Length; i++)
        {
            if((value & NumericValues[i]) != 0)
            {
                //Enum flag is active

                if(builder.Length > 0) builder.Append(", ");
                builder.Append(Names[i]);
            }
        }

        return builder.ToString();
    }

    public Type EnumType { get; }

    public bool IsEnumFlag { get; }

    public string[] StrValues { get; }
    public string[] Names { get; }
    public long[] NumericValues { get; }
}
