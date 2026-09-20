using InsanityLib.Extended.Enums;
using IntegratedModManager.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;

namespace InsanityLib.Auto.Config.IMM.Composers.Simple;

public sealed class EnumComposer : IIMMComposer
{

    public bool CanWriteType(IMMComposerContext context, MemberInfo member, JsonContract contract, out bool requiresAdvanced) => IMMComposerContext.GetNonNullableType(member, out requiresAdvanced).IsEnum;

    public void Write(IMMComposerContext context, MemberInfo member, JsonContract contract, ImmAdvancedSchema? advanced)
    {
        var type = IMMComposerContext.GetNonNullableType(member, out var isNullable);
        if(isNullable) ArgumentNullException.ThrowIfNull(advanced);
        
        var mapper = new EnumNameValueMapping(type);
        
        var serializer = JsonSerializer.CreateDefault();
        serializer.Converters.Add(new ExtendedEnumJsonConverter());
        var options = mapper.GetAllEnumValues().Select(val => new ImmConfigOption
        {
            Label = mapper.GetDisplayString(val),
            Value = JToken.FromObject(Enum.ToObject(type, val), serializer)
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
