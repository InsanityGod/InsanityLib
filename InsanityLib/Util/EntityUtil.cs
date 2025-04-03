using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;

namespace InsanityLib.Util
{
    public static class EntityUtil
    {
        public static EntitySelection GetTargetEntity(this Entity entity)
        {
            if(entity is EntityPlayer player) return player.EntitySelection;

            //TODO something for enemies
            return null;
        }
    }
}
