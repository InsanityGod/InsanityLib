using AutoConfigLib;
using AutoConfigLib.Auto;
using HarmonyLib;
using InsanityLib.Attributes.Auto.Config;
using InsanityLib.Constants;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Util.AutoRegistry
{
    public static class AutoConfig
    {
        //TODO something for syncing config values from server to client

        internal static void LoadAll(IServiceProvider provider)
        {
            var api = provider.GetService<ICoreAPI>();
            var loadModConfig = AccessTools.FirstMethod(typeof(ICoreAPICommon), method => method.Name == nameof(ICoreAPICommon.LoadModConfig) && method.IsGenericMethod);
            var storeModConfig = AccessTools.FirstMethod(typeof(ICoreAPICommon), method => method.Name == nameof(ICoreAPICommon.StoreModConfig) && method.IsGenericMethod);
            foreach ((var member, var attr) in ReflectionUtil.FindAllMembers<AutoConfigAttribute>())
            {
                try
                {
                    if(!(member is FieldInfo || member is PropertyInfo) || !member.IsStatic() || !member.GetPrimaryType().IsComplexClassType()) throw new InvalidOperationException($"{nameof(AutoConfigAttribute)} is only allowed on static fields/properties containing a class");

                    var value = member.GetValue();
                    if (value != null) continue;
                    var configType = member.GetPrimaryType();

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
                    finally
                    {
                        if(value != null && api.ModLoader.IsModEnabled("autoconfiglib")) RegisterToAutoConfigLib(api, value, attr.Path);
                    }
                }
                catch (Exception ex)
                {
                    provider.GetService<ILogger>()?.Error(Logging.ExecutionFailedDefaultTemplate, nameof(AutoConfigAttribute), attr.Path, ex);
                }
            }
        }

        private static void RegisterToAutoConfigLib(ICoreAPI api, object instance, string path)
        {
            AccessTools.Method(typeof(AutoConfigGenerator), nameof(AutoConfigGenerator.RegisterOrCollectConfigFile))
                .MakeGenericMethod(instance.GetType())
                .Invoke(null, new object[] { api, path, instance });
            //TODO move config collection functionality to instanity lib
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
