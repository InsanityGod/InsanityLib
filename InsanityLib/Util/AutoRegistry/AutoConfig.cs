using HarmonyLib;
using InsanityLib.Attributes.Auto.Config;
using InsanityLib.Constants;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace InsanityLib.Util.AutoRegistry
{
    public static class AutoConfig
    {

        internal static void LoadAll(IServiceProvider provider)
        {
            var api = provider.GetService<ICoreAPI>();
            var loadModConfig = AccessTools.FirstMethod(typeof(ICoreAPICommon), method => method.Name == nameof(ICoreAPICommon.LoadModConfig) && method.IsGenericMethod);
            var storeModConfig = AccessTools.FirstMethod(typeof(ICoreAPICommon), method => method.Name == nameof(ICoreAPICommon.StoreModConfig) && method.IsGenericMethod);
            foreach ((var member, var attr) in ReflectionUtil.FindAllMembers<AutoConfigAttribute>())
            {
                try
                {
                    if (member is FieldInfo field && (!field.IsStatic || !field.FieldType.IsClass)) throw new InvalidOperationException($"{nameof(AutoConfigAttribute)} is only allowed on static fields/properties containing a class");
                    if (member is PropertyInfo property && (!(property.GetSetMethod(true)?.IsStatic ?? false) || !property.PropertyType.IsClass)) throw new InvalidOperationException($"{nameof(AutoConfigAttribute)} is only allowed on static fields/properties containing a class");

                    var value = member.GetValue();
                    if (value != null) continue;
                    var configType = member.GetPrimaryType();

                    //TODO test AutoConfigLib compatibility
                    try
                    {
                        value = loadModConfig.MakeGenericMethod(configType)
                            .Invoke(api, new object[] { attr.Path });

                        if(value != null) ValidateAndFix(provider, configType, ref value, attr);

                        if (value == null && attr.CreateIfNotExist)
                        {
                            value = configType.AutoCreate(provider, false);

                            storeModConfig.MakeGenericMethod(configType)
                                .Invoke(api, new object[] { value, attr.Path });
                        }

                        if (value != null) member.SetValue(value);
                    }
                    catch
                    {
                        if (attr.DefaultOnError)
                        {
                            value = configType.AutoCreate(provider, false);
                            if (value != null) member.SetValue(value);
                        }
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    provider.GetService<ILogger>()?.Error(Logging.ExecutionFailedDefaultTemplate, nameof(AutoConfigAttribute), attr.Path, ex);
                }
            }
        }

        private static void ValidateAndFix(IServiceProvider provider, Type configType, ref object configInstance, AutoConfigAttribute configAttr)
        {
            var configChanged = false;
            foreach ((var member, var attr) in ReflectionUtil.FindAllMembers<VersionIdentifierAttribute>(configType))
            {
                //TODO support nested members
                configChanged |= attr.ValidateAndFix(provider, member, ref configInstance, configAttr.Path);
            }

            if (configChanged)
            {
                var api = provider.GetService<ICoreAPI>();
                AccessTools.FirstMethod(typeof(ICoreAPICommon), method => method.Name == nameof(ICoreAPI.StoreModConfig) && method.IsGenericMethod)
                    .MakeGenericMethod(configType)
                    .Invoke(api, new object[] { configInstance, configAttr.Path });
            }

            configInstance.TryNestedValidate(provider, true, true, configAttr.Path).ThrowIfNotValid();
        }
    }
}
