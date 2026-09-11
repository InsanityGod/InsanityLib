using HarmonyLib;
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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.Common;

namespace InsanityLib.Auto.Config.IMM;

public static partial class IMMConfigGenerator
{
    //internal static void GenerateAll(ICoreAPI api)
    //{
    //  //var configGroups = AutoConfig.Loaded.Values
    //  //    .Where(config => !config.IIMConfigGenerated)
    //  //    .GroupBy(config => config.Owner.Info.ModID);
    //  //
    //  //foreach(var group in configGroups)
    //  //{
    //  //    try
    //  //    {
    //  //        Generate(api, group.Key, group);
    //  //    }
    //  //    catch(Exception ex)
    //  //    {
    //  //        api.Logger.Error("[InsanityLib] Failed to generate IIM Config for '{0}', exception: {1}", group.Key, ex);
    //  //    }
    //  //}
    //}

    //internal static void Generate(ICoreAPI api, string modID, IEnumerable<IAutoConfig> configs)
    //{
    //    var location = new AssetLocation(modID, "config/imm.json");
    //    var asset = api.Assets.TryGet(location);
    //  
    //    var IMM = asset?.ToObject<ImmConfigDescriptor>() ?? new();
    //
    //    foreach(var config in configs)
    //    {
    //        if(IMM.Configuration.Any(c => c.ConfigFile == config.RelativePath)) continue;
    //
    //        try
    //        {
    //            IMM.Configuration.Add(Generate(api, config));
    //        }
    //        catch(Exception ex)
    //        {
    //            api.Logger.Error("[InsanityLib] Failed to generate IMM Config for '{0}' from '{1}', exception: {2}", config.RelativePath, modID, ex);
    //        }
    //    }
    //
    //    var settings = new JsonSerializerSettings
    //    {
    //        NullValueHandling = NullValueHandling.Ignore,
    //        Formatting = Formatting.None
    //    };
    //
    //    var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(IMM, settings));
    //    if(asset is null)
    //    {
    //        api.Assets.Add(location, new Asset(location)
    //        {
    //            IsPatched = true,
    //            Data = data
    //        });
    //    }
    //    else asset.Data = data;
    //}

    //internal static ImmConfigBlock Generate(ICoreAPI api, IAutoConfig config)
    //{
    //    var docs = config.AssociatedType.GetDocumentationContext()!;
    //
    //    var IMM = new ImmConfigBlock()
    //    {
    //        ConfigFile = config.RelativePath,
    //        ConfigSource = ImmConfigSource.ModConfig,
    //        ConfigLabel = docs.GetDisplayName(),
    //        Description = docs.GetDescription(),
    //        ConfigSide = config.ServerSync ? ImmConfigSide.Server : ImmConfigSide.Client //TODO configs with ServerSync off technically also exist server side, but they are not editable due to format constraints
    //    };
    //
    //    AppendSettings(IMM, string.Empty, string.Empty, config.AssociatedType);
    //
    //    return IMM;
    //}

    //private static void AppendSettings(ImmConfigBlock IMM, string jsonPath, string labelPath, Type type, MemberInfo? member = null)
    //{
    //
    //    var entry = EntryForMember(member, jsonPath, labelPath);
    //
    //    else if (member.GetCustomAttribute<AllowedValuesAttribute>() is { } allowedValues)
    //    {
    //        entry.Type = "Dropdown";
    //        entry.Options = [.. allowedValues.Values.Select(value => new ImmConfigOption
    //        {
    //            Value = value is null ? JValue.CreateNull() : JToken.FromObject(value),
    //            Label = value is null ? "null / none" : value.ToString()!,
    //        })];
    //    }
    //    else if(JsonSerializer.CreateDefault().ContractResolver.ResolveContract(type) is { } contract)
    //    {
    //        else if(contract.ContractType == JsonContractType.Array && contract is JsonArrayContract arrayContract)
    //        {
    //            if(arrayContract.ItemContract?.ContractType == JsonContractType.String)
    //            {
    //                entry.ElementType = "String";
    //            }
    //            else if((arrayContract.CollectionItemType ?? arrayContract.ItemContract?.UnderlyingType) is Type elementType)
    //            {
    //                if(elementType == typeof(bool))
    //                {
    //                    entry.ElementType = "Boolean";
    //                }
    //                else if (elementType.IsInteger())
    //                {
    //                    entry.ElementType = "Integer";
    //                }
    //                else if(elementType.IsFloatingPoint() || elementType == typeof(decimal))
    //                {
    //                    entry.ElementType = "Decimal";
    //                }
    //            }
    //            if(!string.IsNullOrEmpty(entry.ElementType)) entry.Type = "Array";
    //        }
    //        //TODO Dictionaries
    //    }
    //
    //    if(!string.IsNullOrEmpty(entry?.Type)) IMM.Settings.Add(entry);
    //}
}
