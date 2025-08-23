using InsanityLib.Exceptions;
using InsanityLib.Util;
using System;
using System.ComponentModel.DataAnnotations;
using Vintagestory.API.Common;

namespace InsanityLib.Attributes.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class CollectibleShouldExistAttribute : ValidationAttribute
{
    public EnumItemClass? ItemClass { get; init; }

    public CollectibleShouldExistAttribute(EnumItemClass? itemClass = null)
    {
        ItemClass = itemClass;
    }

    public override bool IsValid(object value)
    {
        if(value is null) return false; //Null will never match a collectible
        if(value is not AssetLocation location) throw new InvalidAttributeUsageException($"[{nameof(CollectibleShouldExistAttribute)}] is only applicable to fields/properties of type {nameof(AssetLocation)}, but was used on {value.GetType()}.");
        var api = ReflectionUtil.GetApi();
        if(api.World.Blocks.Count == 0) return true; //No blocks loaded yet, so we cannot validate //TODO maybe some way to delay validation until blocks are loaded?

        if (ItemClass is null || ItemClass == EnumItemClass.Item)
        {
            if (location.IsWildCard)
            {
                if(api.World.SearchItems(location).Length > 0) return true;
            }
            else if(api.World.GetItem(location) is not null) return true; //Item exists
        }
        
        if(ItemClass is null || ItemClass == EnumItemClass.Block)
        {
            if (location.IsWildCard)
            {
                if(api.World.SearchBlocks(location).Length > 0) return true;
            }
            else if(api.World.GetBlock(location) is not null) return true; //Item exists
        }

        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        var typeStr = ItemClass switch
        {
            EnumItemClass.Item => "items",
            EnumItemClass.Block => "blocks",
            _ => "collectibles"
        };
        return $"'{name}' does not result in any matching {typeStr}";
    }
}
