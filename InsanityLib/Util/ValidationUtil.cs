using HarmonyLib;
using InsanityLib.Constants;
using InsanityLib.Extensions;
using InsanityLib.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Util;

public static class ValidationUtil
{
    public static NestedValidationContext TryNestedValidate(this object obj, IServiceProvider provider, bool tryAutoFix = false, bool logging = false, string identifier = null)
    {
        var nestedContext = new NestedValidationContext
        {
            Provider = provider,
            TryAutoFix = tryAutoFix,
            Logging = logging,
            Identifier = identifier ?? obj?.ToString() ?? "Unknown Object",
        };

        if(obj is not null) NestedValidate(obj, nestedContext, string.Empty);

        return nestedContext;
    }

    private static void NestedValidate(object obj, NestedValidationContext nestedContext, string path)
    {
        if(obj is null || !obj.GetType().IsComplexClassType()) return;
        if(obj is IDictionary dictionary)
        {
            nestedContext.ScannedObjects.Add(obj);
            foreach(DictionaryEntry entry in dictionary)
            {
                NestedValidate(entry.Value, nestedContext, $"{path}/{entry.Key}");
            }
            return;
        }

        if (obj is IEnumerable enumerable)
        {
            nestedContext.ScannedObjects.Add(obj);
            var index = 0;
            foreach (var item in enumerable) NestedValidate(item, nestedContext, $"{path}/{index++}");
            return;
        }

        ILogger logger = nestedContext.Logging ? nestedContext.Provider?.GetService<ILogger>() : null;
        var context = new ValidationContext(obj, nestedContext.Provider, items: null);
        var newResults = new List<ValidationResult>();

        Validator.TryValidateObject(obj, context, newResults, true);
        nestedContext.ScannedObjects.Add(obj);
        
        foreach (var result in newResults)
        {
            if (!nestedContext.TryAutoFix || result.MemberNames.Count() != 1)
            {
                logger?.Warning(Logging.EncounteredValidationError, nestedContext.Identifier, result, $"{path}/({string.Join(", ", result.MemberNames)})");
                nestedContext.Results.Add(result);
                continue;
            }

            var memberName = result.MemberNames.First();
            var members = obj.GetType().GetMember(memberName);
            if (members.Length == 1 && members[0].TryAutoSetDefaultValue(obj, nestedContext.Provider))
            {
                logger?.Warning(Logging.AutoFixSucceed, nestedContext.Identifier, result, $"{path}/{memberName}", members[0].GetValue(obj)); 
                continue;
            }

            logger?.Warning(Logging.AutoFixFailed, nestedContext.Identifier, result, $"{path}/{memberName}");
            nestedContext.Results.Add(result);
        }

        foreach (var member in obj.GetType().GetMembers(AccessTools.all & ~BindingFlags.Static))
        {
            if(!member.CanGetValue()) continue;
            var value = member.GetValue(obj);
            if(value is null || !value.GetType().IsComplexClassType() ||nestedContext.ScannedObjects.Contains(value)) continue;

            NestedValidate(value, nestedContext, $"{path}/{member.Name}");
        }
    }

    public static void EnsureCorrectDomainForAsset(this AssetLocation code, AssetLocation origin, ILogger? logger = null)
    {
        if(code.Domain != origin.Domain)
        {
            logger?.Warning(Logging.DomainDoesNotMatchFileOrigin, origin, code.Domain, code, origin.Domain);
            code.Domain = origin.Domain;
        }
    }
}
