using Vintagestory.API.Common;

namespace InsanityLib.Util;

public static class CollectibleObjectUtil
{
    /// <summary>
    /// Get the CollectibleObject used to place this Block.
    /// Allowes for using the "PlacedByItem" attribute to redirect.
    /// </summary>
    public static CollectibleObject GetPlacedByItem(this Block block, ICoreAPI api)
    {
        if (block.Attributes is not null)
        {
            var redirect = block.Attributes["PlacedByItem"].AsString();
            if (redirect is not null)
            {
                CollectibleObject placedByItem = api.World.GetBlock(redirect);
                placedByItem ??= api.World.GetItem(redirect);
                if (placedByItem is not null) return placedByItem;
                
                api.Logger.Error($"[InsanityLib] Invalid PlacedByItem redirect {block.Code} -> {redirect}");
            }
        }

        return block;
    }

    /// <summary>
    /// Get the Block being placed by this CollectibleObject if any.
    /// Allowes for using the "PlacedBlock" attribute to redirect.
    /// </summary>
    public static Block GetPlacedBlock(this CollectibleObject collectible, ICoreAPI api)
    {
        if (collectible.Attributes is not null)
        {
            var redirect = collectible.Attributes["PlacedBlock"].AsString();
            if (redirect is not null)
            {
                Block block = api.World.GetBlock(redirect);
                if (block is not null) return block;
                
                api.GetService<ILogger>().Error($"[WearAndTear] Invalid PlacedBlock redirect {collectible.Code} -> {redirect}");
            }
        }

        return collectible as Block;
    }

    public static int GetOrientationVariantIndex(this RegistryObject obj)
    {
        int index = obj.VariantStrict.IndexOfKey("side");

        if (index == -1) index = obj.VariantStrict.IndexOfKey("rotation");
        if (index == -1) index = obj.VariantStrict.IndexOfKey("orientation");

        return index;
    }

    public static CollectibleObject GetCollectibleObject(this IWorldAccessor world, AssetLocation code, EnumItemClass? itemType = null)
    {
        CollectibleObject result = null;
        if(itemType != EnumItemClass.Block) result = world.GetItem(code);
        if(itemType != EnumItemClass.Item) result ??= world.GetBlock(code);
        
        return result;
    }
}
