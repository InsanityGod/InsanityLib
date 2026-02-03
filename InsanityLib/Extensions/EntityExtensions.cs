using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace InsanityLib.Extensions;

public static class EntityExtensions
{
    /// <summary>
    /// Gets the entity being targeted by the passed entity.<br/>
    /// Currently only does something when passed a player.
    /// </summary>
    public static EntitySelection? GetTargetEntity(this Entity entity)
    {
        if(entity is EntityPlayer player) return player.EntitySelection;

        //TODO something for enemies maybe
        return null;
    }
}
