using IntegratedModManager.Config;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace InsanityLib.Auto.Config.IMM.Composers.Complex;

public sealed class DictionaryComposer : IIMMComposer
{

    public bool CanWriteType(IMMComposerContext context, MemberInfo member, JsonContract contract, out bool requiresAdvanced)
    {
        requiresAdvanced = true;
        return contract.ContractType == JsonContractType.Dictionary && contract is JsonDictionaryContract;
    }

    public void Write(IMMComposerContext context, MemberInfo member, JsonContract contract, ImmAdvancedSchema? advanced)
    {
        ArgumentNullException.ThrowIfNull(advanced);
        var isNullable = IMMComposerContext.IsNullable(member);
        var dictContract = (JsonDictionaryContract)contract;

        var valType = dictContract.DictionaryValueType ?? dictContract.ItemContract?.UnderlyingType ?? throw new InvalidOperationException("Could not determine dictionary Key type");
        var valComposer = IMMConfigGenerator.FindComposer(context, valType, out _, out _);
        if(valComposer is null)
        {
            advanced.Type = "Object"; // Automatically gets removed as it has no fields
            return;
        }
        
        var valEntry = IMMConfigGenerator.AdvancedEntry(valType);
        valEntry.Key = string.Empty;
        valEntry.Label = string.Empty;
        valComposer.Write(context, valType, dictContract.ItemContract!, valEntry);

        if (!ClassComposer.IsValidEntry(valEntry))
        {
            advanced.Type = "Object"; // Automatically gets removed as it has no fields
            return;
        }

        advanced.Type = "Dictionary";
        advanced.Value = valEntry;
        advanced.Nullable = isNullable;
    }
}
