using InsanityLib.Exceptions;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace InsanityLib.Attributes.Validators
{
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
}
