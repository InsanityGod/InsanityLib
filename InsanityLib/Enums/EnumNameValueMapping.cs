using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Util;

namespace InsanityLib.Enums
{
    public class EnumNameValueMapping
    {

        //TODO extended enum support

        //TODO make enums in configs load/save as string instead of number
        public EnumNameValueMapping(Type enumType, bool includeExtended = true)
        {
            EnumType = enumType;
            IsEnumFlag = enumType.GetCustomAttribute<FlagsAttribute>() != null;
            

            StrValues = Enum.GetNames(enumType);
            
            Names = Enum.GetNames(enumType)
                .Select(Naming.ToHumanReadable)
                .ToArray();

            IntValues = enumType.GetEnumValues()
                .Cast<int>()
                .ToArray();

            if (includeExtended && EnumExtensionUtil.EnumExtensions.TryGetValue(enumType, out var extension))
            {
                foreach((var value, var offset) in extension.OffsetLookup)
                {
                    if(value is string singleRegistry)
                    {
                        var code = singleRegistry.ToAssetLocation(); //TODO maybe an option to include domain between brackets
                        
                        StrValues = StrValues.Append(singleRegistry);
                        Names = Names.Append(code.Path.ToHumanReadable());

                        IntValues = IntValues.Append(offset);
                        continue;
                    }

                    if(value is not Type enumExtensionType || !enumExtensionType.IsEnum) continue;

                    StrValues = StrValues.Append(
                        Enum.GetNames(enumExtensionType)
                    );
                    
                    Names = Names.Append(
                        Enum.GetNames(enumExtensionType)
                            .Select(Naming.ToHumanReadable)
                    );

                    IntValues = IntValues.Append(
                        enumExtensionType.GetEnumValues()
                            .Cast<int>()
                    );

                }
            }


            //TODO filter out Enum values that represent more then 1 flag
        }

        public string[] GetStringValues(object value)
        {
            if (!EnumType.IsInstanceOfType(value)) value = value.AutoConvert(EnumType);
            return Enum.Format(EnumType, value, "G").Split(", ");
        }

        public int[] GetIndexes(object value) => GetStringValues(value)
                .Select(str => Array.IndexOf(StrValues, str))
                .ToArray();

        public string GetDescriptionStrings()
        {
            //TODO also allow for displaying description of the enum values itself
            var builder = new StringBuilder();

            for(var i = 0; i < StrValues.Length; i++)
            {
                builder.Append($"{Names[i]} ({IntValues[i]})");

                if(i != StrValues.Length - 1) builder.Append(", ");
            }

            return builder.ToString();
        }

        public string GetDisplayString(int value)
        {
            if (!IsEnumFlag) return Names[IntValues.IndexOf(value)];
            var builder = new StringBuilder();

            for(var i = 0; i < IntValues.Length; i++)
            {
                if((value & IntValues[i]) != 0)
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
        public int[] IntValues { get; }
    }
}
