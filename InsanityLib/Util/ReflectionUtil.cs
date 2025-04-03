using HarmonyLib;
using InsanityLib.Attributes.Auto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace InsanityLib.Util
{
    public static class ReflectionUtil
    {
        [AutoDefaultValue(null)]
        public static EnumAppSide? LoadedSides { get; internal set; }

        public static bool SideLoaded(EnumAppSide side) => LoadedSides != null && LoadedSides.Value.Is(side);

        // Backing field name pattern: "<PropertyName>k__BackingField"
        public static bool IsBackingField(this MemberInfo field) => field.Name.StartsWith('<') && field.Name.Contains("k__BackingField");
        
        public static object GetValue(this MemberInfo memberInfo, object instance = null) => memberInfo switch
        {
            PropertyInfo property => property.GetValue(instance),
            FieldInfo field => field.GetValue(instance),
            _ => null,
        };

        public static void SetValue(this MemberInfo memberInfo, object value, object instance = null)
        {
            switch (memberInfo)
            {
                case PropertyInfo property:
                    property.SetValue(instance, value);
                    break;
                case FieldInfo field:
                    field.SetValue(instance, value);
                    break;
            }
        }

        /// <summary>
        /// The primary type of this member (whatever type this member provides access to)
        /// </summary>
        public static Type GetPrimaryType(this MemberInfo memberInfo) => memberInfo switch
        {
            PropertyInfo property => property.PropertyType,
            FieldInfo field => field.FieldType,
            MethodInfo method => method.ReturnType,
            _ => null,
        };

        public static IEnumerable<(MemberInfo, T)> FindAllMembers<T>(BindingFlags? flags = null) where T : Attribute => AccessTools.AllTypes()
            .SelectMany(type => type.GetMembers(flags ?? AccessTools.all))
            .Select(member => (member, member.GetCustomAttribute<T>()))
            .Where(pair => pair.Item2 != null);

        public static object Invoke<T>(this T method, object instance = null, object[] parameters = null) where T : MemberInfo => method switch
        {
            MethodInfo info => info.Invoke(instance, parameters),
            _ => throw new InvalidOperationException($"{method} is not a method"),
        };

        public static object AutoInvoke(this object callable, IServiceProvider provider, object instance = null) => callable switch
        {
            MethodBase method => method.Invoke(instance, method.GetAutoParameters(provider)),
            Delegate del => del.DynamicInvoke(del.Method.GetAutoParameters(provider)),
            _ => throw new InvalidOperationException("Not a callable object"),
        };

        public static object[] GetAutoParameters(this MethodBase method, IServiceProvider provider)
        {
            var parameterInfo = method.GetParameters();
            var parameters = new object[parameterInfo.Length];

            for (var i = 0; i < parameterInfo.Length; i++)
            {
                var info = parameterInfo[i];
                //TODO allow for manually filling in gaps
                parameters[i] = provider.GetService(info.ParameterType);
                if(info.HasDefaultValue) parameters[i] ??= info.DefaultValue;
            }

            return parameters;
        }

    }
}
