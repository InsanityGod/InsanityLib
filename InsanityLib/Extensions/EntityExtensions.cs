using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace InsanityLib.Extensions;

public static class EntityExtensions
{
    public static EntitySelection GetTargetEntity(this Entity entity)
    {
        if(entity is EntityPlayer player) return player.EntitySelection;

        //TODO something for enemies
        return null;
    }
}
