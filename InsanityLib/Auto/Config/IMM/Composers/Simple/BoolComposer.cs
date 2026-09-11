using IntegratedModManager.Config;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace InsanityLib.Auto.Config.IMM.Composers.Simple;

public sealed class BoolComposer : IIMMComposer
{

    public bool CanWriteType(IMMComposerContext context, MemberInfo member, JsonContract contract, out bool requiresAdvanced) => IMMComposerContext.GetNonNullableType(member, out requiresAdvanced) == typeof(bool);

    public void Write(IMMComposerContext context, MemberInfo member, JsonContract contract, ImmAdvancedSchema? advanced)
    {
        var isNullable = IMMComposerContext.IsNullable(member);
        if(isNullable) ArgumentNullException.ThrowIfNull(advanced);

        if(advanced is not null)
        {
            advanced.Type = "Boolean";
            advanced.Nullable = isNullable;
        }
        else context.IMMConfig.Settings.Add(IMMConfigGenerator.TopLevelEntry(member, "Boolean"));
    }
}
