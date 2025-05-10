using InsanityLib.Config.Util;
using ConfigLib;
using HarmonyLib;
using InsanityLib.Attributes.Auto;
using InsanityLib.Attributes.Auto.Config;
using InsanityLib.Config;
using InsanityLib.Constants;
using InsanityLib.Enums.Auto.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace InsanityLib.Util.AutoRegistry
{
    public static class AutoConfig
    {
        [AutoClear] //TODO move config tracking code to this library from AutoConfigLib
        internal static Dictionary<string, AutoConfigLib> LoadedConfigs { get; } = new();

        private static bool TryAssignFromCache(string path, MemberInfo member)
        {
            if(!LoadedConfigs.TryGetValue(path, out var value)) return false;
            member.SetValue(value.ConfigInstance);
            return true;
        }

        public static void StoreModConfig(ICoreAPI api, object value, string filename)
        {
            if (InsanityLibConfig.Instance != null && !InsanityLibConfig.Instance.AutoConfig.DocumentationInConfigFile)
            {
                AccessTools.FirstMethod(typeof(ICoreAPICommon), method => method.Name == nameof(ICoreAPICommon.StoreModConfig) && method.IsGenericMethod)
                    .MakeGenericMethod(value.GetType())
                    .Invoke(api, new object[] { value, filename });
                return;
            }

            FileInfo fileInfo = new(Path.Combine(GamePaths.ModConfig, filename));
			GamePaths.EnsurePathExists(fileInfo.Directory.FullName);
            
            var settings = JsonConvert.DefaultSettings?.Invoke() ?? new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.Converters.Add(new JsonConverterWithCommentInjection());

            string json = JsonConvert.SerializeObject(value, settings);
			File.WriteAllText(fileInfo.FullName, json);
        }


        internal static void LoadAll(IServiceProvider provider)
        {
            var api = provider.GetService<ICoreAPI>();
            var loadModConfig = AccessTools.FirstMethod(typeof(ICoreAPICommon), method => method.Name == nameof(ICoreAPICommon.LoadModConfig) && method.IsGenericMethod);
            
            foreach ((var member, var attr) in ReflectionUtil.FindAllMembers<AutoConfigAttribute>().OrderByDescending(config => config.Item1.GetPrimaryType() == typeof(InsanityLibConfig))) //Ensure primary config is loaded first
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
                            var jsonBase64 = clientApi.World.Config.GetOrAddTreeAttribute("insanitylib").GetString(attr.Path);
                            value = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(Convert.FromBase64String(jsonBase64)), configType) ?? throw new InvalidOperationException($"Config is configured to be synced from server but no config was sent for '{attr.Path}'");
                            member.SetValue(value);
                        }
                        else
                        {
                            value = loadModConfig.MakeGenericMethod(configType)
                                .Invoke(api, new object[] { attr.Path });

                            if(value != null) ValidateAndFix(provider, configType, ref value, attr);

                            if (value == null && attr.CreateIfNotExist)
                            {
                                value = configType.AutoCreate(provider, false);

                                if(InsanityLibConfig.Instance == null || !InsanityLibConfig.Instance.AutoConfig.SaveOnLoad) StoreModConfig(api, value, attr.Path);
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
                            LoadedConfigs.Add(attr.Path, new AutoConfigLib(api, value, attr));

                            if(InsanityLibConfig.Instance.AutoConfig.SaveOnLoad) StoreModConfig(api, value, attr.Path);
                        }

                        if (attr.ServerSync && api is ICoreServerAPI serverAPI) //Register even if playing singleplayer, since opening to LAN is a thing
                        {
                            //TODO use this same mechanism to allow for localizing configs to be world specific
                            var json = JsonConvert.SerializeObject(value, Formatting.None);
                            serverAPI.World.Config.GetOrAddTreeAttribute("insanitylib").SetString(attr.Path, Convert.ToBase64String(Encoding.UTF8.GetBytes(json)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    provider.GetService<ILogger>()?.Error(Logging.ExecutionFailedDefaultTemplate, nameof(AutoConfigAttribute), attr.Path, ex);
                }
            }

            if(api is ICoreClientAPI) RegisterToConfigLib(api);
        }

        internal static void RegisterToConfigLib(ICoreAPI api)
        {
            if(!api.ModLoader.IsModEnabled("configlib")) return;
            foreach ((_, var config) in LoadedConfigs)
            {
                RegisterToConfigLib(api, config); //Seperate method to avoid crash when configlib is not present
            }
        }

        private static void RegisterToConfigLib(ICoreAPI api, AutoConfigLib config)
        {
            if (config.ConfigLibMode == EConfigLibMode.Never) return;
            if (config.ConfigLibMode == EConfigLibMode.OnlyWithAutoConfigLib && !api.ModLoader.IsModEnabled("autoconfiglib")) return;
            
            var configLib = api.ModLoader.GetModSystem<ConfigLibModSystem>();

            configLib.RegisterCustomConfig(config.Path, (string domain, ControlButtons buttons) =>
            {
                //TODO handle server and client difference
                if(buttons.Save) config.Save();
                if(buttons.Restore) config.Restore();
                if(buttons.Defaults) config.Defaults();
                if(buttons.Reload) config.Reload();

                config.Render();
            });
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
                if(InsanityLibConfig.Instance == null || !InsanityLibConfig.Instance.AutoConfig.SaveOnLoad) StoreModConfig(api, configInstance, configAttr.Path);
            }

            configInstance.TryNestedValidate(provider, true, true, configAttr.Path).ThrowIfNotValid();
        }
    }
}
