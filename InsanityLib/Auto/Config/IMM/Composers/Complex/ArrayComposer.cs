using IntegratedModManager.Config;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace InsanityLib.Auto.Config.IMM.Composers.Complex;

public sealed class ArrayComposer : IIMMComposer
{

    public bool CanWriteType(IMMComposerContext context, MemberInfo member, JsonContract contract, out bool requiresAdvanced)
    {
        requiresAdvanced = true;
        return contract.ContractType == JsonContractType.Array && contract is JsonArrayContract;
    }

    public void Write(IMMComposerContext context, MemberInfo member, JsonContract contract, ImmAdvancedSchema? advanced)
    {
        ArgumentNullException.ThrowIfNull(advanced);
        var isNullable = IMMComposerContext.IsNullable(member);
        var arrayContract = (JsonArrayContract)contract;
        var childType = arrayContract.CollectionItemType ?? arrayContract.ItemContract?.UnderlyingType ?? throw new InvalidOperationException("Could not determine collection item type");
        var composer = IMMConfigGenerator.FindComposer(context, childType, out _, out _);
        if(composer is null)
        {
            advanced.Type = "Object"; // Automatically gets removed as it has no fields
            return;
        }
        
        var entry = IMMConfigGenerator.AdvancedEntry(childType);
        entry.Key = string.Empty;
        entry.Label = string.Empty;
        composer.Write(context, childType, arrayContract.ItemContract!, entry);
        if (!ClassComposer.IsValidEntry(entry))
        {
            advanced.Type = "Object"; // Automatically gets removed as it has no fields
            return;
        }

        advanced.Type = "Array";
        advanced.Element = entry;
        advanced.Nullable = isNullable;
    }
}
