using InsanityLib.Extensions;
using IntegratedModManager.Config;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace InsanityLib.Auto.Config.IMM.Composers;

public sealed class IMMComposerContext
{
    public required ImmConfigBlock IMMConfig { get; init; }
    public required IAutoConfig? AutoConfig { get; init; }
    public required IContractResolver ContractResolver { get; init; }

    public static bool IsNullable(MemberInfo member)
    {
        if(member.GetCustomAttribute<RequiredAttribute>() is not null) return false;
        
        return member.GetNullableState() != NullabilityState.NotNull;
    }

    /// <summary>
    /// For ensuring you have the actual type and not the nullable wrapper or the likes
    /// </summary>
    public static Type GetNonNullableType(MemberInfo info, out bool wasNullable)
    {
        var type = info.GetPrimaryType()!;
        if(Nullable.GetUnderlyingType(type) is Type underlyingType)
        {
            wasNullable = true;
            return underlyingType;
        }

        wasNullable = false;
        return type;
    }
}
