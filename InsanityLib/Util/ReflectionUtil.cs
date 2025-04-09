using HarmonyLib;
using InsanityLib.Attributes.Auto;
using InsanityLib.Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;

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

        public static bool CanAutoInvoke(this MethodBase method, IServiceProvider provider)
        {
            var parameters = method.GetParameters();

            foreach(var param in parameters)
            {
                var service = provider.GetService(param.ParameterType);
                if (service == null && !param.HasDefaultValue) return false;
            }
            return true;
        }

        public static object AutoInvoke(this object callable, IServiceProvider provider, object instance = null) => callable switch
        {
            MethodBase method => method.Invoke(instance, method.GetAutoParameters(provider)),
            Delegate del => del.DynamicInvoke(del.Method.GetAutoParameters(provider)),
            _ => throw new InvalidOperationException("Not a callable object"),
        };

        public static T AutoCreate<T>(this IServiceProvider provider, bool returnNullOnFailure = true) where T : class => (T)typeof(T).AutoCreate(provider, returnNullOnFailure);

        public static object AutoCreate(this Type type, IServiceProvider provider, bool returnNullOnFailure = true)
        {
            //TODO maybe create a custom attribute to specify default auto constructor
            var constructors = type.GetConstructors();
            ConstructorInfo bestConstructor = null;
            object[] bestParameters = null;
            int maxParams = -1;

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                var paramValues = new object[parameters.Length];
                int paramCount = 0;

                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    var service = provider.GetService(param.ParameterType);

                    if(service != null) paramValues[i] = service;
                    else if (param.HasDefaultValue) paramValues[i] = param.DefaultValue;
                    else break;
                    paramCount++;
                }

                if(paramCount != parameters.Length || paramCount <= maxParams) continue; 

                bestConstructor = constructor;
                bestParameters = paramValues;
                maxParams = paramCount;
            }

            if (bestConstructor == null)
            {
                if(returnNullOnFailure) return null;
                throw new InvalidOperationException($"No suitable constructor found for type {type.FullName}");
            }

            return bestConstructor.Invoke(bestParameters);
        }

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

        public static bool IsNotNull(this object value) => value != null;


        public static object FindMatch(this Type type, IEnumerable<object> objects)
        {
            object bestMatch = null;

            foreach(var obj in objects)
            {
                var objType = obj.GetType();
                if (objType == type) return obj; //Exact match
                if(Array.Exists(objType.GetInterfaces(), interfaceType => interfaceType == type)) return obj; //Exact interface match
                if (type.IsAssignableFrom(objType)) bestMatch ??= obj; //Best match //TODO maybe think of a better way to judge what is the best match
            }

            return bestMatch;
        }
    }
}
