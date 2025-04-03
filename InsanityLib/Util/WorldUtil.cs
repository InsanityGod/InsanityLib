using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace InsanityLib.Util
{
    public static class WorldUtil
    {
        public static Block GetOrFindBlock(this BlockSelection blockSel, IWorldAccessor world) => blockSel.Block ??= world.BlockAccessor.IsValidPos(blockSel.Position) ? world.BlockAccessor.GetBlock(blockSel.Position) : null;
        
        public static BlockEntity FindBlockEntity(this BlockSelection blockSel, IWorldAccessor world) => world.BlockAccessor.IsValidPos(blockSel.Position) ? world.BlockAccessor.GetBlockEntity(blockSel.Position) : null;
    }
}
