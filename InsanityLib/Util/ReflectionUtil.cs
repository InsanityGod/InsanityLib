using HarmonyLib;
using InsanityLib.Attributes.Auto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Util
{
    public static class ReflectionUtil
    {
        [AutoDefaultValue(null)]
        public static EnumAppSide? LoadedSides { get; internal set; }

        public static bool SideLoaded(EnumAppSide side) => LoadedSides is not null && LoadedSides.Value.Is(side);

        public static ICoreAPI GetApi(bool prioritizeServer = true)
        {
            ICoreAPI result = prioritizeServer ? InsanityLibModSystem.GlobalServiceContainer.GetService<ICoreServerAPI>() : InsanityLibModSystem.GlobalServiceContainer.GetService<ICoreClientAPI>();
            result ??= prioritizeServer ? InsanityLibModSystem.GlobalServiceContainer.GetService<ICoreClientAPI>() : InsanityLibModSystem.GlobalServiceContainer.GetService<ICoreServerAPI>();
            return result;
        }

        // Backing field name pattern: "<PropertyName>k__BackingField"
        public static bool IsBackingField(this MemberInfo field) => field.Name.StartsWith('<') && field.Name.Contains("k__BackingField");
        
        public static bool IsComplexClassType(this Type type) => !type.IsValueType && type != typeof(string) && type != typeof(Delegate) && !typeof(MethodBase).IsAssignableFrom(type);

        public static bool IsStatic(this MemberInfo info) => info switch
        {
            PropertyInfo property => (property.GetMethod ?? property.SetMethod).IsStatic,
            FieldInfo field => field.IsStatic,
            MethodBase method => method.IsStatic,
            _ => false,
        };

        public static object GetValue(this MemberInfo memberInfo, object instance = null) => memberInfo switch
        {
            PropertyInfo property => property.GetValue(instance),
            FieldInfo field => field.GetValue(instance),
            _ => null,
        };

        public static bool CanGetValue(this MemberInfo memberInfo) => memberInfo switch
        {
            PropertyInfo property => property.CanRead && property.GetIndexParameters().Length == 0,
            FieldInfo => true,
            _ => false,
        };

        public static object GetAutoValue(this MemberInfo memberInfo, IServiceProvider provider, object instance = null) => memberInfo switch
        {
            PropertyInfo property => property.GetValue(instance),
            FieldInfo field => field.GetValue(instance),
            MethodBase method => method.AutoInvoke(provider, instance),
            _ => null,
        };

        public static bool CanGetAutoValue(this MemberInfo memberInfo, IServiceProvider provider, object instance = null) => memberInfo switch
        {
            PropertyInfo property => property.CanRead && property.GetIndexParameters().Length == 0,
            FieldInfo => true,
            MethodBase method => method.CanAutoInvoke(provider),
            _ => false,
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

        public static bool TryAutoSetValue(this MemberInfo member, object value, object instance = null)
        {
            try
            {
                member.SetValue(value.AutoConvert(member.GetPrimaryType()), instance);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool CanSetValue(this MemberInfo memberInfo) => memberInfo switch
        {
            PropertyInfo property => property.CanWrite && property.GetIndexParameters().Length == 0,
            FieldInfo => true,
            _ => false,
        };

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

        public static T TryGetCustomAttribute<T>(this MemberInfo member) where T : Attribute
        {
            try
            {
                return member.GetCustomAttribute<T>();
            }
            catch
            {
                return default;
            }
        }

        public static IEnumerable<(MemberInfo, T)> FindAllMembers<T>(BindingFlags? flags = null) where T : Attribute => AccessTools.AllTypes()
            .SelectMany(type => type.GetMembers(flags ?? AccessTools.all))
            .Select(member => (member, member.TryGetCustomAttribute<T>()))
            .Where(pair => pair.Item2 is not null);

        public static IEnumerable<(MemberInfo, T)> FindAllMembers<T>(Type type, BindingFlags? flags = null) where T : Attribute => type
            .GetMembers(flags ?? AccessTools.all)
            .Select(member => (member, member.TryGetCustomAttribute<T>()))
            .Where(pair => pair.Item2 is not null);

        public static IEnumerable<(Type, T)> FindAllClasses<T>() where T : Attribute => AccessTools.AllTypes()
            .Select(type => (type, type.TryGetCustomAttribute<T>()))
            .Where(pair => pair.Item2 is not null);


        public static IEnumerable<Type> FindImplementations<T>(this Assembly assembly, bool includeSelf = false) =>
            AccessTools.GetTypesFromAssembly(assembly)
            .Where(type =>  !type.IsAbstract && !type.IsInterface && typeof(T).IsAssignableFrom(type) && (includeSelf || type != typeof(T)));

        public static Type FindGenericInterfaceDefinition(this Type type, Type genericInterfaceType) =>
            type.GetInterfaces()
            .SingleOrDefault(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == genericInterfaceType);

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
                if (service is null && !param.HasDefaultValue) return false;
            }
            return true;
        }

        public static object AutoInvoke(this object callable, IServiceProvider provider, object instance = null) => callable switch
        {
            MethodBase method => method.Invoke(instance, method.GetAutoParameters(provider)),
            Delegate del => del.DynamicInvoke(del.Method.GetAutoParameters(provider)),
            _ => throw new InvalidOperationException("Not a callable object"),
        };

        public static bool TryAutoSetDefaultValue(this MemberInfo member, object instance, IServiceProvider provider)
        {
            if(!member.CanSetValue()) return false;

            try
            {
                var defaultAttr = member.TryGetCustomAttribute<DefaultValueAttribute>();
                member.SetAutoDefaultValue(defaultAttr, instance, provider);
                return true;
            }
            catch { /* fail silently */ }

            return false;
        }

        public static void SetAutoDefaultValue(this MemberInfo member, IServiceProvider provider, object instance = null) => member.SetAutoDefaultValue(member.TryGetCustomAttribute<DefaultValueAttribute>(), instance, provider);

        //TODO method for just getting the auto default value (so we can use this on method parameters)
        public static void SetAutoDefaultValue(this MemberInfo member, DefaultValueAttribute defaultAttr, object instance, IServiceProvider provider)
        {
            if(defaultAttr is not null)
            {
                var value = defaultAttr is AutoDefaultValueAttribute autoDefaultAttr
                    ? autoDefaultAttr.GetAutoDefaultValue(provider, instance)
                    : defaultAttr.Value;
                
                member.SetValue(value.AutoConvert(member.GetPrimaryType()), instance);
            }
        }

        public static T AutoCreate<T>(this IServiceProvider provider, bool returnNullOnFailure = true) where T : class => (T)typeof(T).AutoCreate(provider, returnNullOnFailure);

        public static object AutoCreate(this Type type, IServiceProvider provider, bool returnNullOnFailure = true)
        {
            if (type.IsValueType) return type.Default();
            if(type == typeof(string)) return string.Empty;

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

                    if(service is not null) paramValues[i] = service;
                    else if (param.HasDefaultValue) paramValues[i] = param.DefaultValue;
                    else break;
                    paramCount++;
                }

                if(paramCount != parameters.Length || paramCount <= maxParams) continue; 

                bestConstructor = constructor;
                bestParameters = paramValues;
                maxParams = paramCount;
            }

            if (bestConstructor is null)
            {
                if(returnNullOnFailure) return null;
                throw new InvalidOperationException($"No suitable constructor found for type {type.FullName}");
            }

            return bestConstructor.Invoke(bestParameters);
        }

        /// <summary>
        /// Retrieves the parameters for a method, automatically resolving them from the provided service provider.
        /// </summary>
        /// <param name="method">The method for which to get the parameters.</param>
        /// <param name="provider">The service provider used to resolve the parameters.</param>
        /// <returns>An array of resolved parameters.</returns>
        public static object[] GetAutoParameters(this MethodBase method, IServiceProvider provider)
        {
            var parameterInfo = method.GetParameters();
            var parameters = new object[parameterInfo.Length];

            for (var i = 0; i < parameterInfo.Length; i++)
            {
                var info = parameterInfo[i];
                //TODO allow for manually filling in gaps
                parameters[i] = provider.GetService(info.ParameterType);
                if (info.HasDefaultValue) parameters[i] ??= info.DefaultValue;
            }

            return parameters;
        }

        /// <summary>
        /// Helper method to check if an object is null (only usefull if you need a prediction delegate)
        /// </summary>
        /// <returns>True if the object is not null else false.</returns>
        public static bool IsNotNull(this object value) => value is not null;

        /// <summary>
        /// Finds the best match for a type in a list of objects.
        /// </summary>
        /// <param name="type">The type you are searching for.</param>
        /// <param name="objects">The objects to search through.</param>
        /// <param name="filter">Extra filter that it has to fullfill</param>
        /// <returns>The best matching object, or null if no match is found.</returns>
        public static T FindMatch<T>(this Type type, IEnumerable<T> objects, System.Func<T, bool> filter = null)
        {
            T bestMatch = default;

            foreach(var obj in objects)
            {
                if(filter is not null && !filter.Invoke(obj)) continue;

                var objType = obj.GetType();
                if (objType == type) return obj; //Exact match
                if(Array.Exists(objType.GetInterfaces(), interfaceType => interfaceType == type)) return obj; //Exact interface match
                if (type.IsAssignableFrom(objType)) bestMatch ??= obj; //Best match //TODO maybe think of a better way to judge what is the best match
            }

            return bestMatch;
        }

        /// <summary>
        /// Recursively searches for a property or field by its name in the given object and retrieves its value.
        /// </summary>
        /// <param name="obj">The object to crawl through.</param>
        /// <param name="path">The target path, with properties/fields separated by '/'.</param>
        /// <param name="result">The last found value while crawling the path, this may contain an exception if failure occured during retrieval property/field</param>
        /// <param name="flags">The flags used to search for members that can be traversed</param>
        /// <returns>The part that could not be crawled</returns>
        public static ReadOnlySpan<char> TryCrawl(this object obj, ReadOnlySpan<char> path, out object result, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty)
        {
            while (!path.IsEmpty)
            {
                if (obj is null) break;

                var seperatorIndex = path.IndexOf('/');
                var nextPart = seperatorIndex == -1 ? path : path[..seperatorIndex];

                var member = obj.GetType().GetMember(nextPart.ToString(), flags).SingleOrDefault();
                if(member is null) break;
                
                //TODO indexer support?
                try
                {
                    obj = member.GetValue(obj);
                }
                catch(Exception ex)
                {
                    obj = ex;
                    break;
                }

                path = seperatorIndex == -1 ? ReadOnlySpan<char>.Empty : path[(seperatorIndex + 1)..]; //Skip the seperator
            }
            
            result = obj;
            return path;
        }

        public static int GetRandom<T>(bool allowExtendedValues = false, Random random = null) where T : Enum
        {
            if(allowExtendedValues) throw new NotImplementedException("random values do not support extended values yet"); //TODO
            random ??= Random.Shared;
            
            var valid = Enum.GetValues(typeof(T));
            return (int)valid.GetValue(random.Next(valid.Length));
        }
    }
}
