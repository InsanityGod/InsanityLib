using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InsanityLib.Validation;

public class NestedValidationContext
{
    internal NestedValidationContext() { }

    public IServiceProvider Provider { get; init; }
    public HashSet<object> ScannedObjects { get; init; } = new HashSet<object>();
    public List<ValidationResult> Results { get; init; } = new List<ValidationResult>();
    public bool TryAutoFix { get; init; } = false;
    public bool Logging { get; init; } = false;

    public string Identifier { get; init; } = "Unknown Object";

    /// <summary>
    /// Wether the object is valid
    /// </summary>
    public bool IsValid => Results.Count == 0;

    public void ThrowIfNotValid()
    {
        if(!IsValid)
        {
            throw new ValidationException($"Validation failed for {Identifier}:\n{string.Join("\n", Results)}");
        }
    }

    //TODO some way to find out whether auto fixing was required to make it valid
}
