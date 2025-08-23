using ConfigLib;
using HarmonyLib;
using ImGuiNET;
using InsanityLib.Attributes.Auto;
using InsanityLib.Attributes.Auto.Config;
using InsanityLib.Config;
using InsanityLib.Config.Util;
using InsanityLib.Constants;
using InsanityLib.Enums.Auto.Config;
using InsanityLib.Interfaces.UI.ImGuiComponents;
using InsanityLib.UI.ImGuiTools;
using InsanityLib.UI.ImGuiTools.Components.Util;
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

namespace InsanityLib.Util.AutoRegistry;

public static class AutoConfigUtil
{
    [AutoClear]
    internal static Dictionary<string, AutoConfig> LoadedConfigs { get; } = new();

    private static bool TryAssignFromCache(string path, MemberInfo member)
    {
        if(!LoadedConfigs.TryGetValue(path, out var value)) return false;
        member.SetValue(value.ConfigInstance);
        return true;
    }

    public static void StoreModConfig(ICoreAPI api, object value, string filename)
    {
        if (InsanityLibConfig.Instance is not null && !InsanityLibConfig.Instance.AutoConfig.DocumentationInConfigFile)
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

    public static void RegisterConfigLibEvents(ICoreAPI api)
    {
        if(api.Side == EnumAppSide.Server) return;

        var configLib = api.ModLoader.GetModSystem<ConfigLibModSystem>();
        configLib.ConfigWindowClosed += AutoConfig.Cleanup;
    }


    internal static void LoadAll(IServiceProvider provider)
    {
        var api = provider.GetService<ICoreAPI>();
        var loadModConfig = AccessTools.FirstMethod(typeof(ICoreAPICommon), method => method.Name == nameof(ICoreAPICommon.LoadModConfig) && method.IsGenericMethod);
        if(api.ModLoader.IsModEnabled("configlib")) RegisterConfigLibEvents(api);
        foreach ((var member, var attr) in ReflectionUtil.FindAllMembers<AutoConfigAttribute>().OrderByDescending(config => config.Item1.GetPrimaryType() == typeof(InsanityLibConfig))) //Ensure primary config is loaded first
        {
            try
            {
                if(!(member is FieldInfo || member is PropertyInfo) || !member.IsStatic() || !member.GetPrimaryType().IsComplexClassType()) throw new InvalidOperationException($"{nameof(AutoConfigAttribute)} is only allowed on static fields/properties containing a class");

                var value = member.GetValue();
                if ((api.Side == EnumAppSide.Client && value is not null) || TryAssignFromCache(attr.Path, member)) continue;
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

                        if(value is not null) ValidateAndFix(provider, configType, ref value, attr);

                        if (value is null && attr.CreateIfNotExist)
                        {
                            value = configType.AutoCreate(provider, false);

                            if(InsanityLibConfig.Instance is null || !InsanityLibConfig.Instance.AutoConfig.SaveOnLoad) StoreModConfig(api, value, attr.Path);
                        }

                        if (value is not null) member.SetValue(value);
                    }
                }
                catch
                {
                    if (attr.DefaultOnError)
                    {
                        value = configType.AutoCreate(provider, false);
                        if (value is not null) member.SetValue(value);
                    }
                    throw;
                }
                finally
                {
                    if(value is not null)
                    {
                        LoadedConfigs.Add(attr.Path, new AutoConfig(api, value, attr));

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
        
        var mode = InsanityLibConfig.Instance.AutoConfig.ConfigUIMode;
        if(mode == EConfigEditorMode.NoConfigEditor) return;

        if (!api.ModLoader.IsModEnabled("autoconfiglib"))
        {
            api.Logger.Warning(Logging.ModRequirementNotMetDefaulting, nameof(EConfigEditorMode.AutoConfigLibEditor), nameof(EConfigEditorMode.InsanityLibConfigEditor), "autoconfiglib");
            mode = EConfigEditorMode.InsanityLibConfigEditor;
        }
        
        foreach ((_, var config) in LoadedConfigs)
        {
            RegisterToConfigLib(api, config, config.ConfigInstance is InsanityLibConfig ? EConfigEditorMode.InsanityLibConfigEditor : mode);
        }
    }

    private static void RegisterToAutoConfigLib(ICoreAPI api, object instance, string path)
    {
        AccessTools.Method(typeof(AutoConfigLib.Auto.AutoConfigGenerator), nameof(AutoConfigLib.Auto.AutoConfigGenerator.RegisterOrCollectConfigFile))
            .MakeGenericMethod(instance.GetType())
            .Invoke(null, new object[] { api, path, instance });
    }

    public static Popup BlockingPopup { get; set; }

    public static void NotifyUserOfException(Exception ex, IImGuiComponent component)
    {
        var clientApi = InsanityLibModSystem.GlobalServiceContainer.GetService<ICoreClientAPI>();
        var context = new ImGuiContext(component, null, id: "ErrorPopup", serviceProvider: clientApi.GetServiceContainer());
        BlockingPopup = new Popup(context)
        {
            Title = ex.Message,
            Text = ex.ToString(),
            AcceptLabel = "Continue##ErrorPopup-Continue",
            RejectLabel = "Copy Error And Continue##ErrorPopup-Copy-Continue",
            RejectCallback = () => clientApi.Forms.SetClipboardText(ex.ToString())
        };
    }

    private static void RegisterToConfigLib(ICoreAPI api, AutoConfig config, EConfigEditorMode UIMode)
    {
        switch (UIMode)
        {
            case EConfigEditorMode.AutoConfigLibEditor:
                RegisterToAutoConfigLib(api, config.ConfigInstance, config.Path);
                break;

            case EConfigEditorMode.InsanityLibConfigEditor:
                var configLib = api.ModLoader.GetModSystem<ConfigLibModSystem>();

                configLib.RegisterCustomConfig(config.Path, (domain, buttons) =>
                {
                    var serverConfigOnClient = !ReflectionUtil.SideLoaded(EnumAppSide.Server) && config.ServerSync;
                    
                    ImGui.BeginDisabled(serverConfigOnClient);
                    if (serverConfigOnClient)
                    {
                        ImGui.Text("Client side editing of server config is not supported yet");
                        ImGui.NewLine();
                    }

                    if (buttons.Save) config.Save();
                    if (buttons.Restore) config.Restore(true);
                    
                    //TODO discard changes method
                    if (buttons.Defaults) config.Defaults();
                    if (buttons.Reload) config.Reload();

                    config.Render();
                    if(BlockingPopup is not null)
                    {
                        BlockingPopup.SafeRender();
                        if(!BlockingPopup.IsOpen) BlockingPopup = null;
                    }

                    ImGui.EndDisabled();
                    return new ControlButtons
                    {
                        Save = !serverConfigOnClient, //Only server can save for now
                        Restore = !serverConfigOnClient,
                        Defaults = !serverConfigOnClient,
                        Reload = false
                    };
                });
                break;
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
            if(InsanityLibConfig.Instance is null || !InsanityLibConfig.Instance.AutoConfig.SaveOnLoad) StoreModConfig(api, configInstance, configAttr.Path);
        }

        configInstance.TryNestedValidate(provider, true, true, configAttr.Path).ThrowIfNotValid();
    }
}
