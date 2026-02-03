using Vintagestory.API.Common;

namespace InsanityLib.Extensions;

public static class WorldExtensions
{
    public static Block? GetOrFindBlock(this BlockSelection blockSel, IWorldAccessor world) => blockSel.Block ??= world.BlockAccessor.IsValidPos(blockSel.Position) ? world.BlockAccessor.GetBlock(blockSel.Position) : null;
    
    public static BlockEntity? FindBlockEntity(this BlockSelection blockSel, IWorldAccessor world) => world.BlockAccessor.IsValidPos(blockSel.Position) ? world.BlockAccessor.GetBlockEntity(blockSel.Position) : null;
}
