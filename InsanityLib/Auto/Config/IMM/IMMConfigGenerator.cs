using Cairo;
using InsanityLib.Auto.Config.IMM.Composers;
using InsanityLib.Auto.Config.IMM.Composers.Complex;
using InsanityLib.Auto.Config.IMM.Composers.Simple;
using InsanityLib.Documentation;
using InsanityLib.Extended.Enums;
using InsanityLib.Extensions;
using InsanityLib.Util;
using IntegratedModManager.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.Common;

namespace InsanityLib.Auto.Config.IMM;

public static partial class IMMConfigGenerator
{
    internal static readonly IIMMComposer[] Composers = [
        new ArrayComposer(),
        new DictionaryComposer(),
        new DropDownComposer(),
        new BoolComposer(),
        new EnumComposer(),
        new IntegerComposer(),
        new FloatingPointComposer(),
        new StringComposer(),
        new ClassComposer()
    ];

    internal static void GenerateAll(ICoreAPI api)
    {
        var configGroups = AutoConfig.Loaded.Values
            .Where(config => !config.IIMConfigGenerated)
            .GroupBy(config => config.Owner.Info.ModID);
        
        foreach(var group in configGroups)
        {
            try
            {
                GenerateAndAppend(api, group.Key, group);
            }
            catch(Exception ex)
            {
                api.Logger.Error("[InsanityLib] Failed to generate IIM Config for '{0}', exception: {1}", group.Key, ex);
            }
        }
    }

    internal static void GenerateAndAppend(ICoreAPI api, string modID, IEnumerable<IAutoConfig> configs)
    {
        var location = new AssetLocation(modID, "config/imm.json");
        var asset = api.Assets.TryGet(location);
      
        var IMM = asset?.ToObject<ImmConfigDescriptor>() ?? new();

        foreach(var config in configs)
        {
            IMM.Configuration.RemoveAll(c => c.ConfigFile == config.RelativePath);

            try
            {
                IMM.Configuration.Add(Generate(config));
            }
            catch(Exception ex)
            {
                api.Logger.Error("[InsanityLib] Failed to generate IMM Config for '{0}' from '{1}', exception: {2}", config.RelativePath, modID, ex);
            }
        }

        var settings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore, //TODO a lot of unnecesary values are still getting serialized (prob because IMM does not annotate them as default values)
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None
        };

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(IMM, settings));
        if(asset is null)
        {
            api.Assets.Add(location, new Asset(location)
            {
                IsPatched = true,
                Data = data
            });
        }
        else asset.Data = data;
    }

    public static void GenerateAndAppend(ICoreAPI api, Mod mod, Type type, string relativePath, EnumAppSide side)
    {
        var location = new AssetLocation(mod.Info.ModID, "config/imm.json");
        var asset = api.Assets.TryGet(location);

        var IMM = asset?.ToObject<ImmConfigDescriptor>() ?? new();
        if(IMM.Configuration.Any(config => config.ConfigFile == relativePath)) return; //Already exists

        try
        {
            var entry = GenerateForType(relativePath, side, type);
            IMM.Configuration.Add(entry);

            var settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore, //TODO a lot of unnecesary values are still getting serialized (prob because IMM does not annotate them as default values)
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };

            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(IMM, settings));
            if(asset is null)
            {
                api.Assets.Add(location, new Asset(location)
                {
                    IsPatched = true,
                    Data = data
                });
            }
            else asset.Data = data;
        }
        catch(Exception ex)
        {
            api.Logger.Error("[InsanityLib] Failed to generate IMM Config for '{0}' from '{1}', exception: {2}", relativePath, mod.Info.ModID, ex);
        }
    }

    public static ImmConfigBlock Generate(IAutoConfig config)
    {
        var docs = config.AssociatedType.GetDocumentationContext()!;

        var context = new IMMComposerContext
        {
            AutoConfig = config,
            ContractResolver = JsonSerializer.CreateDefault().ContractResolver,
            IMMConfig = new ImmConfigBlock
            {
                ConfigFile = config.RelativePath,
                ConfigSource = ImmConfigSource.ModConfig,
                ConfigLabel = docs.GetDisplayName(),
                Description = docs.GetDescription(),
                ConfigSide = config.ServerSync ? ImmConfigSide.Server : ImmConfigSide.Client //TODO configs with ServerSync off technically also exist server side, but they are not editable due to format constraints
            }
        };

        ClassComposer.WriteTopLevel(context, config.AssociatedType);

        return context.IMMConfig;
    }

    public static ImmConfigBlock GenerateForType(string relativeConfigPath, EnumAppSide side, Type type, bool useExtendedEnumSyntax = false)
    {
        if(side == EnumAppSide.Universal) throw new InvalidOperationException("IMM configs need to be either Server or Client owned");

        var docs = type.GetDocumentationContext()!;

        var context = new IMMComposerContext
        {
            AutoConfig = null,
            ContractResolver = JsonSerializer.CreateDefault().ContractResolver,
            UseExtendedEnumSyntax = useExtendedEnumSyntax,
            IMMConfig = new ImmConfigBlock
            {
                ConfigFile = relativeConfigPath,
                ConfigSource = ImmConfigSource.ModConfig,
                ConfigLabel = docs.GetDisplayName(),
                Description = docs.GetDescription(),
                ConfigSide = side == EnumAppSide.Server ? ImmConfigSide.Server : ImmConfigSide.Client
            }
        };

        ClassComposer.WriteTopLevel(context, type);

        return context.IMMConfig;
    }

    public static IIMMComposer? FindComposer(IMMComposerContext context, MemberInfo member, out JsonProperty? property, out JsonContract contract, out bool requiresAdvanced)
    {
        var type = member.GetPrimaryType()!;
        
        if(member.DeclaringType is not null && context.ContractResolver.ResolveContract(member.DeclaringType!) is JsonObjectContract parentContract)
        {
            property = parentContract.Properties.FirstOrDefault(prop => prop.UnderlyingName == member.Name);
        }
        else property = null;
        contract = property?.PropertyContract ?? context.ContractResolver.ResolveContract(type);

        foreach (var composer in Composers)
        {
            if(composer.CanWriteType(context, member, property, contract, out requiresAdvanced))
            {
                return composer;
            }
        }

        requiresAdvanced = false;
        return null;
    }

    public static ImmConfigEntry TopLevelEntry(JsonProperty? property, MemberInfo member, string type)
    {
        var docs = member.GetDocumentationContext();
        var key = property?.PropertyName ?? member.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? member.Name;
        var name = docs?.GetDisplayName() ?? member.Name.ToHumanReadable();

        return new ImmConfigEntry
        {
            Label = name,
            Map = key,
            Type = type,
            Description = docs?.GetDescription() ?? string.Empty,
        };
    }

    public static ImmAdvancedSchema AdvancedEntry(JsonProperty? property, MemberInfo member)
    {
        var docs = member.GetDocumentationContext();
        var key = property?.PropertyName ?? member.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? member.Name;
        var name = docs?.GetDisplayName() ?? member.Name.ToHumanReadable();

        JToken? defaultValue = null;

        if(member.GetCustomAttribute<DefaultValueAttribute>() is { } defaultAttr)
        {
            try
            {
                if(defaultAttr.Value is null)
                {
                    defaultValue = null;
                }
                else if(property?.Converter is { } converter)
                {
                    using var writer = new JTokenWriter();
                    var serializer = JsonSerializer.CreateDefault();

                    converter.WriteJson(writer, defaultAttr.Value.AutoConvert(member.GetPrimaryType())!, serializer);
                    defaultValue = writer.Token;
                }
                else
                {
                    defaultValue = JToken.FromObject(defaultAttr.Value.AutoConvert(member.GetPrimaryType())!);
                }

                //defaultValue = defaultAttr.Value is null ? JValue.CreateNull() : JToken.FromObject(defaultAttr.Value.AutoConvert(member.GetPrimaryType())!);
            }
            catch 
            {
                //Ignore for now
            }
        }

        return new ImmAdvancedSchema
        {
            Label = name,
            Key = key,
            Description = docs?.GetDescription() ?? string.Empty,
            InitialValue = defaultValue,
        };
    }
}
