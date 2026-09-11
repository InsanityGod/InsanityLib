using InsanityLib.Extensions;
using IntegratedModManager.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace InsanityLib.Auto.Config.IMM.Composers.Complex;

public sealed class ClassComposer : IIMMComposer
{
    public bool CanWriteType(IMMComposerContext context, MemberInfo member, JsonContract contract, out bool requiresAdvanced)
    {
        requiresAdvanced = true;
        return contract.ContractType == JsonContractType.Object;
    }

    public static void WriteTopLevel(IMMComposerContext context, Type type)
    {
        foreach(var group in GetValidMembers(type))
        {
            foreach(var member in group)
            {
                if (IMMConfigGenerator.FindComposer(context, member, out var contract, out var requiresAdvanced) is not { } composer) continue;

                ImmConfigEntry? entry = null;
                if (requiresAdvanced)
                {
                    entry = IMMConfigGenerator.TopLevelEntry(member, "Advanced");
                    entry.Advanced = new ImmAdvancedSchema();
                }
                composer.Write(context, member, contract, entry?.Advanced);

                if (IsValidEntry(entry))
                {
                    context.IMMConfig.Settings.Add(entry);
                }
            }
        }
    }

    public void Write(IMMComposerContext context, MemberInfo member, JsonContract contract, ImmAdvancedSchema? advanced)
    {
        ArgumentNullException.ThrowIfNull(advanced);
        advanced.Type = "Object";

        foreach(var group in GetValidMembers(member.GetPrimaryType()!))
        {
            foreach(var groupMember in group)
            {
                if (IMMConfigGenerator.FindComposer(context, groupMember, out var childContract, out _) is not { } composer) continue;

                var entry = IMMConfigGenerator.AdvancedEntry(groupMember);
                composer.Write(context, groupMember, childContract, entry);

                if (IsValidEntry(entry))
                {
                    advanced.Fields.Add(entry);
                }
            }
        }
    }

    public static bool IsValidEntry([NotNullWhen(true)] ImmAdvancedSchema? entry)
    {
        return entry is not null && (entry.Type != "Object" || entry.Fields.Count > 0);
    }

    public static bool IsValidEntry([NotNullWhen(true)] ImmConfigEntry? entry)
    {
        return entry is not null && (entry.Advanced?.Type != "Object" || entry.Advanced.Fields.Count > 0);
    }

    public static IOrderedEnumerable<IGrouping<string?, MemberInfo>> GetValidMembers(Type type) => type
        .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.InvokeMethod)
        .Where(IsValidMember)
        .GroupBy(member => member.GetCustomAttribute<CategoryAttribute>()?.Category)
        .OrderByDescending(group => group.Key is null)
        .ThenBy(group => group.Key);
    
    private static bool IsValidMember(MemberInfo member)
    {
        if (member.DeclaringType == typeof(object) || !member.CanGetValue() || !member.CanSetValue() || member.GetPrimaryType() == typeof(object) || member.IsBackingField()) return false;
        if (
            member.GetCustomAttribute<JsonExtensionDataAttribute>() is not null
            || member.GetCustomAttribute<JsonIgnoreAttribute>() is not null
            || member.GetCustomAttribute<BrowsableAttribute>() is { Browsable: false }
            || member.GetCustomAttribute<ReadOnlyAttribute>() is { IsReadOnly: true }
        ) return false;

        return true;
    }
}
