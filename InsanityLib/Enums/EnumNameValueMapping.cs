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

        public Type EnumType { get; }

        public bool IsEnumFlag { get; }

        public string[] StrValues { get; }
        public string[] Names { get; }
        public int[] IntValues { get; }
    }
}
