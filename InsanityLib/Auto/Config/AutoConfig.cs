using InsanityLib.Config;
using InsanityLib.Extended.Enums;
using InsanityLib.Extensions;
using InsanityLib.Generators.Attributes;
using InsanityLib.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace InsanityLib.Auto.Config;
//TODO maybe some events to hook into loading logic

public sealed class AutoConfig<T> : IAutoConfig<T> where T : class, new()
{
    private readonly Action<T?>? OnLoad;
    private T? configInstance;

    internal AutoConfig(string relativePath, Action<T?>? onLoad = null, bool serverSync = false)
    {
        RelativePath = relativePath;
        ServerSync = serverSync;
        OnLoad = onLoad;
    }

    public T? ConfigInstance
    {
        get => configInstance;
        private set
        {
            if(value == configInstance) return;
            configInstance = value;
            OnLoad?.Invoke(configInstance);
        }
    }

    public string RelativePath { get; }

    public bool ServerSync { get; }

    public bool IsLocalized { get; private set; } //TODO Public methods for toggeling this

    public bool TryLoadConfig(ICoreAPI api, ILogger logger)
    {
        //Load from localized configs (world config)
        var localizedConfigBytes = api.World.Config.GetTreeAttribute("insanitylib_localized_configs")?.GetBytes(RelativePath);
        IsLocalized = localizedConfigBytes is not null;
        if(IsLocalized)
        {
            try
            {
                ConfigInstance = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(localizedConfigBytes!));
                return true;
            }
            catch(Exception ex)
            {
                logger.Error("Failed to load localized config '{0}' from worldconfig, using default configuration: {1}", RelativePath, ex);
                ConfigInstance = new();
                return false;
            }
        }

        var configBytes = api.World.Config.GetTreeAttribute("insanitylib_configs")?.GetBytes(RelativePath);

        //Load from worldconfig (send by server)
        if (api.Side == EnumAppSide.Client && ServerSync)
        {
            if (configBytes is null)
            {
                logger.Error("Expected server to send config '{0}' but nothing was receiver, using default configuration", RelativePath);
                ConfigInstance = new();
                return false;
            }

            try
            {
                ConfigInstance = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(configBytes));
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Failed to load server config '{0}' from worldconfig, using default configuration: {1}", RelativePath, ex);
                ConfigInstance = new();
                return false;
            }
        }

        //Load from disk
        try
        {
            ConfigInstance = api.LoadModConfig<T>(RelativePath);
            ValidateAndFix(api.GetServiceProvider());
        }
        catch(Exception ex)
        {
            logger.Error("Failed to load config '{0}' from disk, using default configuration: {1}", RelativePath, ex);
            ConfigInstance = new();
            return false;
        }
        
        if(InsanityLibConfig.Instance is null) api.ModLoader.GetModSystem<InsanityLibModSystem>().EnsureConfigLoaded(api);
        if (InsanityLibConfig.Instance!.AutoConfig.SaveOnLoad) TrySaveConfig(api, logger);
        return true;
    }

    public bool TrySaveConfig(ICoreAPI api, ILogger logger) => AutoConfig.TrySaveConfig(ConfigInstance, typeof(T), RelativePath, api, logger);

    public void ValidateAndFix(IServiceProvider provider)
    {
        if(ConfigInstance is null) return;

        var configVersionUpgrade = false;
        var tmpConfig = ConfigInstance!;
        foreach ((var member, var attr) in typeof(T).FindAllMembersWithAttributes<VersionIdentifierAttribute>())
        {
            configVersionUpgrade |= attr.ValidateAndUpgrade(provider, member, ref tmpConfig, RelativePath);
        }

        tmpConfig.TryNestedValidate(provider, true, true, RelativePath).ThrowIfNotValid();

        ConfigInstance = tmpConfig;
    }
}

public static class AutoConfig
{
    [AutoClear]
    public static Dictionary<string, IAutoConfig> Loaded { get; } = [];

    public static IAutoConfig<T> GetOrRegister<T>(ICoreAPI api, ILogger logger, string path, bool serverSync = false, bool eagerLoad = true, Action<T?>? onLoad = null) where T : class, new()
    {
        path = path.EnsureFileExtension(".json");
        if(Loaded.TryGetValue(path, out var config))
        {
            if(config.AssociatedType != typeof(T)) throw new InvalidOperationException($"Attempt to load config '{path}' with type '{typeof(T)}' but it's actually type '{config.AssociatedType}'");
        }
        else
        {
            config = new AutoConfig<T>(path, onLoad, serverSync);
            Loaded[path] = config;
        }

        var typedConfig = (IAutoConfig<T>)config;

        if(eagerLoad && typedConfig.ConfigInstance is null) typedConfig.TryLoadConfig(api, logger);

        if(api.ModLoader.IsModEnabled("configlib")) new ConfigLib.AutoConfigLib(api, typedConfig).RegisterToConfigLib(api);

        return typedConfig;
    }

    internal static bool TrySaveConfig(object? configInstance, Type configInstanceType, string relativePath, ICoreAPI api, ILogger logger)
    {
        if(configInstance is null)
        {
            logger.Warning("Failed to save config '{0}' of type '{1}' as it is not loaded yet", relativePath, configInstanceType);
            return false;
        }

        try
        {
            FileInfo fileInfo = new(Path.Combine(GamePaths.ModConfig, relativePath));
            GamePaths.EnsurePathExists(fileInfo.Directory!.FullName);
        
            var settings = JsonConvert.DefaultSettings?.Invoke() ?? new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;

            if (InsanityLibConfig.Instance!.AutoConfig.DocumentationInConfigFile)
            {
                settings.Converters.Add(new JsonConverterWithCommentInjection());
            }
            settings.Converters.Add(new ExtendedEnumJsonConverter());

            using var stream = File.CreateText(fileInfo.FullName);
            using var writer = new JsonTextWriter(stream)
            {
                Formatting = settings.Formatting,
            };
            
            var serializer = JsonSerializer.Create(settings);
            serializer.Serialize(writer, configInstance);

            return true;
        }
        catch(Exception ex)
        {
            logger.Error("Failed to save config '{0}' of type '{1}': ", relativePath, configInstanceType, ex);
            return false;
        }
    }
}