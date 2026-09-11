using InsanityLib.Extensions;
using IntegratedModManager.Config;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace InsanityLib.Auto.Config.IMM.Composers.Simple;

public sealed class StringComposer : IIMMComposer
{
    public static readonly Type[] PrimitiveStringTypes = [
        typeof(string),
        typeof(Guid),
        typeof(DateTime)
    ];
 
    public bool CanWriteType(IMMComposerContext context, MemberInfo member, JsonContract contract, out bool requiresAdvanced)
    {
        var result = (contract.ContractType == JsonContractType.String || PrimitiveStringTypes.Contains(member.GetPrimaryType())); //TODO dropdown component
        requiresAdvanced = result && IMMComposerContext.IsNullable(member);
        return result;
    }

    public void Write(IMMComposerContext context, MemberInfo member, JsonContract contract, ImmAdvancedSchema? advanced)
    {
        var isNullable = IMMComposerContext.IsNullable(member);
        if(isNullable) ArgumentNullException.ThrowIfNull(advanced);

        if(advanced is not null)
        {
            advanced.Type = "String";
            advanced.Nullable = isNullable;
        }
        else context.IMMConfig.Settings.Add(IMMConfigGenerator.TopLevelEntry(member, "String"));
    }
}
