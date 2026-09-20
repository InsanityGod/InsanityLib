using InsanityLib.Extensions;
using IntegratedModManager.Config;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace InsanityLib.Auto.Config.IMM.Composers.Complex;

public sealed class DropDownComposer : IIMMComposer
{

    public bool CanWriteType(IMMComposerContext context, MemberInfo member, JsonContract contract, out bool requiresAdvanced)
    {
        requiresAdvanced = IMMComposerContext.IsNullable(member);
        return member.GetCustomAttribute<AllowedValuesAttribute>() is not null;
    }

    public void Write(IMMComposerContext context, MemberInfo member, JsonContract contract, ImmAdvancedSchema? advanced)
    {
        var isNullable = IMMComposerContext.IsNullable(member);
        if(isNullable) ArgumentNullException.ThrowIfNull(advanced);

        var attr = member.GetCustomAttribute<AllowedValuesAttribute>() ?? throw new ArgumentException("Requires an AllowedValuesAttribute", nameof(member));
        var type = member.GetPrimaryType();

        var options = attr.Values.Select(obj => obj.AutoConvert(type)).Select(obj => new ImmConfigOption
        {
            Label = obj is null ? "Null" : obj.ToString() ?? "None",
            Value = obj is null ? JValue.CreateNull() : JToken.FromObject(obj),
        }).ToList();

        if(advanced is not null)
        {
            advanced.Type = "Dropdown";
            advanced.Nullable = isNullable;
            advanced.Options = options;
        }
        else
        {
            var entry = IMMConfigGenerator.TopLevelEntry(member, "Dropdown");
            entry.Options = options;
            context.IMMConfig.Settings.Add(entry);
        }
    }
}
