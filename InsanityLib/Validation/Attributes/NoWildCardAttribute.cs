using InsanityLib.Exceptions;
using System;
using System.ComponentModel.DataAnnotations;
using Vintagestory.API.Common;

namespace InsanityLib.Validation.Attributes;

/// <summary>
/// Validates that the given <see cref="AssetLocation"/> does not use a wildcard.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class NoWildCardAttribute(string errorMessage = "'{0}' is not allowed to use a wildcard") : ValidationAttribute(errorMessage)
{
    public override bool IsValid(object value)
    {
        if(value is null) return true; //Null will never match a collectible
        if(value is not AssetLocation location) throw new InvalidAttributeUsageException($"[{nameof(NoWildCardAttribute)}] is only applicable to fields/properties of type {nameof(AssetLocation)}, but was used on {value.GetType()}.");
        return !location.IsWildCard;
    }
}
