using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.Client.NoObf;

namespace InsanityLib.Extended
{
    public class ExtendedEnum
    {
        public Type EnumType { get; }

        protected readonly OrderedDictionary<object, int> OffsetLookup = new();
        protected int currentOffset;

        public ExtendedEnum(Type enumType)
        {
            if(enumType.GetCustomAttribute<FlagsAttribute>() != null) throw new InvalidOperationException($"Cannot extend {enumType}: Enum extension is not supported for flags");
            EnumType = enumType;
            currentOffset = Enum.GetValues(enumType).Cast<int>().Max() + 1;

        }

        public void RegisterExtension<T>() where T : Enum
        {
            var enumType = typeof(T);
            if (OffsetLookup.ContainsKey(enumType)) return;
            if(enumType.GetCustomAttribute<FlagsAttribute>() != null) throw new InvalidOperationException($"Cannot extend {enumType}: Enum extension is not supported for flags");
            
            var offset = Enum.GetValues(enumType).Cast<int>().Max() + 1;
            OffsetLookup[enumType] = currentOffset;
            currentOffset += offset;
            //TODO
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
                if(obj is string strObj && string.Equals(strObj, value, StringComparison.OrdinalIgnoreCase)) return offset; //TODO maybe store as asset location as well?
                
                if(obj is not Type type) continue;
                var name =  Array.Find(Enum.GetNames(type), name => string.Equals(name, value, StringComparison.OrdinalIgnoreCase));
                if(name == null) continue;

                return (int)Enum.Parse(type, name) + offset;
            }

            return null;
        }
    }
}
