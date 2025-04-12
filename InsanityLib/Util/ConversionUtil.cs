using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace InsanityLib.Util
{
    public static class ConversionUtil
    {
        public static T AutoConvert<T>(this object value) => (T)value.AutoConvert(typeof(T));

        public static object AutoConvert(this object value, Type targetType)
        {
            if(value == null || targetType.IsInstanceOfType(value)) return value;

            // Handle enums
            if (targetType.IsEnum && value is string str)
            {
                try
                {
                    return Enum.Parse(targetType, str);
                }
                catch { /* fail silently */ }
            }

            // Handle other types
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch { /* fail silently */ }

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
                if(notNullableType != null) return value.AutoConvert(notNullableType);
                else throw;
            }
        }

        internal static T CastWrapper<T>(object value) => (T)value;

        public static object Cast(this object value, Type targetType) => AccessTools.Method(typeof(ConversionUtil), nameof(CastWrapper))
            .MakeGenericMethod(targetType)
            .Invoke(null, new object[] { value });
    }
}
