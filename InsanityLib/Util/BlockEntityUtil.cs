using InsanityLib.Behaviors.BlockEntityBehaviors;
using InsanityLib.Contexts;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace InsanityLib.Util
{
    public static class BlockEntityUtil
    {
        public static PermanentBehaviorManager GetOrCreatePermanentBehaviorManager(this BlockEntity blockEntity)
        {
            var manager = blockEntity.GetBehavior<PermanentBehaviorManager>();
            if (manager == null)
            {
                manager = new PermanentBehaviorManager(blockEntity);
                blockEntity.Behaviors.Add(manager);
            }
            return manager;
        }

        public static bool RemovePermanentBehavior(this BlockEntity blockEntity, BlockEntityBehavior behavior) => blockEntity.GetBehavior<PermanentBehaviorManager>()?.RemoveBehavior(behavior) ?? false;
        public static bool RemovePermanentBehavior(this BlockEntity blockEntity, string key) => blockEntity.GetBehavior<PermanentBehaviorManager>()?.RemoveBehavior(key) ?? false;

        public static BlockEntityBehavior TryAddPermanentbehavior(this BlockEntity blockEntity, string behavior, JsonObject properties = null)
        {
            if(blockEntity.Api.Side != EnumAppSide.Server) return null;
            properties ??= new JsonObject(new JObject());
            var manager = blockEntity.GetOrCreatePermanentBehaviorManager();
            
             var base64Properties = Convert.ToBase64String(Encoding.UTF8.GetBytes(properties.ToString()));
            
            try
            {
                if (blockEntity.Api.ClassRegistry.GetBlockEntityBehaviorClass(behavior) == null)
                {
                    blockEntity.Api.GetService<ILogger>().Warning(Lang.Get("Failed to add permanent BlockEntityBehavior {0} for {1}", behavior, blockEntity.Pos));
                    return null;
                }
        
                var blockEntityBehavior = blockEntity.Api.ClassRegistry.CreateBlockEntityBehavior(blockEntity, behavior);
                blockEntityBehavior.properties = properties;
                
                manager.Behaviors.Add(Guid.NewGuid().ToString(), new PermanentBlockEntityBehaviorContext
                {
                    Name = behavior,
                    Base64EncodedProperties = base64Properties,
                    Instance = blockEntityBehavior
                });
                blockEntity.Behaviors.Add(blockEntityBehavior);
                blockEntityBehavior.Initialize(blockEntity.Api, properties);
                blockEntity.MarkDirty();

                return blockEntityBehavior;
            }
            catch(Exception ex)
            {
                blockEntity.Api.GetService<ILogger>().Error(Lang.Get("Failed to add permanent BlockEntityBehavior {0} for {1}, ex: {2}", behavior, blockEntity.Pos, ex));
            }
            return null;
        }
    }
}
