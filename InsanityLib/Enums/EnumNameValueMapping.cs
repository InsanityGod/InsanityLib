using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Enums
{
    public class EnumNameValueMapping
    {
        //TODO extended enum support
        public EnumNameValueMapping(Type enumType)
        {
            EnumType = enumType;
            //TODO filter out Enum values that represent more then 1 flag
            StrValues = Enum.GetNames(enumType);
            Names = Enum.GetNames(enumType)
                .Select(Naming.ToHumanReadable)
                .ToArray();

            IsEnumFlag = enumType.GetCustomAttribute<FlagsAttribute>() != null;
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
                builder.Append($"{Names[i]} ({(int)Enum.Parse(EnumType, StrValues[i])})");

                if(i != StrValues.Length - 1) builder.Append(", ");
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
