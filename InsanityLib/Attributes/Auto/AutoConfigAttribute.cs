using HarmonyLib;
using InsanityLib.Constants;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory;
using Vintagestory.API.Common;

namespace InsanityLib.Attributes.Auto
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AutoConfigAttribute : AutoDefaultValueAttribute
    {
        public string Path { get; set; }
        
        public bool DefaultOnLoadError { get; set; }
        
        public bool CreateIfNotExist { get; set; }

        public AutoConfigAttribute(string path, bool defaultOnLoadError = true, bool createIfNotExist = true)
        {
            Path = path; //TODO ensure this ends in .json
            DefaultOnLoadError = defaultOnLoadError;
            CreateIfNotExist = createIfNotExist;
        }

        public static void LoadAll(IServiceProvider provider)
        {
            var api = provider.GetService<ICoreAPI>();
            var loadModConfig = AccessTools.FirstMethod(api.GetType(), method => method.Name == nameof(ICoreAPI.LoadModConfig) && method.IsGenericMethod);
            var storeModConfig = AccessTools.FirstMethod(api.GetType(), method => method.Name == nameof(ICoreAPI.StoreModConfig) && method.IsGenericMethod);
            foreach ((var member, var attr) in ReflectionUtil.FindAllMembers<AutoConfigAttribute>())
            {
                try
                {
                    if(member is FieldInfo field && (!field.IsStatic || !field.FieldType.IsClass)) throw new InvalidOperationException($"{nameof(AutoConfigAttribute)} is only allowed on static fields/properties containing a class");
                    if(member is PropertyInfo property && (!(property.GetSetMethod()?.IsStatic ?? false) || !property.PropertyType.IsClass)) throw new InvalidOperationException($"{nameof(AutoConfigAttribute)} is only allowed on static fields/properties containing a class");
                    
                    var value = member.GetValue();
                    if(value != null) continue;
                    var configType = member.GetPrimaryType();

                    //TODO test AutoConfigLib compatibility
                    try
                    {
                        value = loadModConfig.MakeGenericMethod(configType)
                            .Invoke(api, new object[] { attr.Path });

                        if(value == null && attr.CreateIfNotExist)
                        {
                            value = configType.AutoCreate(provider, false);

                            storeModConfig.MakeGenericMethod(configType)
                                .Invoke(api, new object[] { value, attr.Path });
                        }

                        if(value != null) member.SetValue(value);
                    }
                    catch
                    {
                        if (attr.DefaultOnLoadError)
                        {
                            value = configType.AutoCreate(provider, false);
                            if(value != null) member.SetValue(value);
                        }
                        throw;
                    }
                }
                catch(Exception ex)
                {
                    provider.GetService<ILogger>()?.Error(Logging.ExecutionFailedDefaultTemplate, nameof(AutoConfigAttribute), attr.Path, ex);
                }
            }
        }
    }
}
