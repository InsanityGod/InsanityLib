using AutoConfigLib.Auto;
using HarmonyLib;
using InsanityLib.Attributes.Auto;
using InsanityLib.Attributes.Auto.Config;
using InsanityLib.Config;
using InsanityLib.Constants;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace InsanityLib.Util.AutoRegistry
{
    public static class AutoConfig
    {
        [AutoClear] //TODO move config tracking code to this library from AutoConfigLib
        private static Dictionary<string, object> LoadedConfigs { get; } = new();

        private static bool TryAssignFromCache(string path, MemberInfo member)
        {
            if(!LoadedConfigs.TryGetValue(path, out var value)) return false;
            member.SetValue(value);
            return true;
        }

        //TODO make non generic
        public static void StoreModConfig<T>(ICoreAPI api, T value, string filename)
        {
            FileInfo fileInfo = new(Path.Combine(GamePaths.ModConfig, filename));
			GamePaths.EnsurePathExists(fileInfo.Directory.FullName);
            var settings = JsonConvert.DefaultSettings?.Invoke() ?? new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.Converters.Add(new JsonWithCommentsConverter());

            string json = JsonConvert.SerializeObject(value, settings);
            ////string json2 = JsonConvert.SerializeObject(value);
            ////api.StoreModConfig<T>(value, filename);
            //new JsonSerializerSettings
            //{
            //    Formatting = Formatting.Indented,
            //    Converters = { new JsonWithCommentsConverter() },
            //}
			File.WriteAllText(fileInfo.FullName, json);
        }


        internal static void LoadAll(IServiceProvider provider)
        {
            var api = provider.GetService<ICoreAPI>();
            var loadModConfig = AccessTools.FirstMethod(typeof(ICoreAPICommon), method => method.Name == nameof(ICoreAPICommon.LoadModConfig) && method.IsGenericMethod);
            var storeModConfig = AccessTools.Method(typeof(AutoConfig), nameof(StoreModConfig)); //AccessTools.FirstMethod(typeof(ICoreAPICommon), method => method.Name == nameof(ICoreAPICommon.StoreModConfig) && method.IsGenericMethod);
            foreach ((var member, var attr) in ReflectionUtil.FindAllMembers<AutoConfigAttribute>())
            {
                try
                {
                    if(!(member is FieldInfo || member is PropertyInfo) || !member.IsStatic() || !member.GetPrimaryType().IsComplexClassType()) throw new InvalidOperationException($"{nameof(AutoConfigAttribute)} is only allowed on static fields/properties containing a class");

                    var value = member.GetValue();
                    if (value != null || TryAssignFromCache(attr.Path, member)) continue;
                    var configType = member.GetPrimaryType();

                    try
                    {
                        if(attr.ServerSync && api is ICoreClientAPI clientApi && !clientApi.IsSinglePlayer)
                        {
                            var json = clientApi.World.Config.GetOrAddTreeAttribute("insanitylib").GetString(attr.Path);
                            value = JsonConvert.DeserializeObject(json, configType) ?? throw new InvalidOperationException($"Config is configured to be synced from server but no config was sent for '{attr.Path}'");
                        }
                        else
                        {
                            value = loadModConfig.MakeGenericMethod(configType)
                                .Invoke(api, new object[] { attr.Path });

                            if(value != null) ValidateAndFix(provider, configType, ref value, attr);

                            if (value == null && attr.CreateIfNotExist)
                            {
                                value = configType.AutoCreate(provider, false);

                                storeModConfig.MakeGenericMethod(configType)
                                    //.Invoke(api, new object[] { value, attr.Path });
                                    .Invoke(null, new object[] { api, value, attr.Path });
                            }

                            if (value != null) member.SetValue(value);
                        }
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
                        if(value != null)
                        {
                            LoadedConfigs.Add(attr.Path, value);
                            if(api.ModLoader.IsModEnabled("autoconfiglib")) RegisterToAutoConfigLib(api, value, attr.Path);
                        }

                        if (attr.ServerSync && api is ICoreServerAPI serverAPI) //Register even if playing singleplayer, since opening to LAN is a thing
                        {
                            //TODO use this same mechanism to allow for localizing configs to be world specific
                            var json = JsonConvert.SerializeObject(value, Formatting.None);
                            serverAPI.World.Config.GetOrAddTreeAttribute("insanitylib").SetString(attr.Path, json);
                        }
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
                //AccessTools.FirstMethod(typeof(ICoreAPICommon), method => method.Name == nameof(ICoreAPI.StoreModConfig) && method.IsGenericMethod)
                AccessTools.Method(typeof(AutoConfig), nameof(StoreModConfig))
                    .MakeGenericMethod(configType)
                    //.Invoke(api, new object[] { configInstance, configAttr.Path });
                    .Invoke(null, new object[] { api, configInstance, configAttr.Path });
            }

            configInstance.TryNestedValidate(provider, true, true, configAttr.Path).ThrowIfNotValid();
        }
    }
}
