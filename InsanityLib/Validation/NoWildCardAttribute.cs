using InsanityLib.Exceptions;
using System;
using System.ComponentModel.DataAnnotations;
using Vintagestory.API.Common;

namespace InsanityLib.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class NoWildCardAttribute : ValidationAttribute
{

    public NoWildCardAttribute()
    {
    }

    public override bool IsValid(object value)
    {
        if(value is null) return true; //Null will never match a collectible
        if(value is not AssetLocation location) throw new InvalidAttributeUsageException($"[{nameof(NoWildCardAttribute)}] is only applicable to fields/properties of type {nameof(AssetLocation)}, but was used on {value.GetType()}.");
        return !location.IsWildCard;
    }

    public override string FormatErrorMessage(string name) => $"'{name}' is not allowed to use a wildcard";
}
