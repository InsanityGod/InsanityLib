using HarmonyLib;
using InsanityLib.Behaviors.BlockEntityBehaviors;
using InsanityLib.Contexts;
using InsanityLib.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;
using Vintagestory.Server;

namespace InsanityLib.HarmonyPatches.LogicChanges
{
    [HarmonyPatch]
    public static class PermanentBlockEntityBehaviorPatches
    {

        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ClientChunk), "PreLoadBlockEntitiesFromPacket");
            yield return AccessTools.Method(typeof(BlockSchematic), nameof(BlockSchematic.TransformWhilePacked));
            yield return AccessTools.Method(typeof(ClientSystemEntities), "UpdateBlockEntityData");
            yield return AccessTools.Method(typeof(ServerChunk), "AfterDeserialization");
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AppendPermanentBehaviors(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();

            var fromTreeAttributesMethod = AccessTools.Method(typeof(BlockEntity), nameof(BlockEntity.FromTreeAttributes));
            var createBehaviorsMethod = AccessTools.Method(typeof(BlockEntity), nameof(BlockEntity.CreateBehaviors));
            
            var fromTreeAttributesIndex = codes.FindIndex(code => code.Calls(fromTreeAttributesMethod));

            var createBehaviorsIndex = codes.FindIndex(code => code.Calls(createBehaviorsMethod));
            codes.InsertRange(createBehaviorsIndex, new CodeInstruction[]
            {
                codes[fromTreeAttributesIndex - 3].Clone(), //BlockEntity
                codes[createBehaviorsIndex - 2].Clone(), //Tree
                codes[createBehaviorsIndex - 1].Clone(), //World
                new(OpCodes.Call, AccessTools.Method(typeof(PermanentBlockEntityBehaviorPatches), nameof(CreatePermanentBehaviors)))
            });

            return codes;
        }



        public static void CreatePermanentBehaviors(BlockEntity blockEntity, ITreeAttribute tree, IWorldAccessor worldForResolve)
        {
            //TODO maybe a command for cleaning this forcibly
            //TODO add a way to add runtime
            //TODO save these so they actually become permanent
            var permanentBehaviorTree = tree.GetTreeAttribute("permanent-behaviors");
            if (permanentBehaviorTree == null) return;
            var manager = new PermanentBehaviorManager(blockEntity);
            
            foreach ((var key, var attr) in permanentBehaviorTree)
            {
                if (attr is not TreeAttribute behaviorTree) continue;

                var name = behaviorTree.GetAsString("name");
                var base64Properties = behaviorTree.GetAsString("properties");
                try
                {
                    if (worldForResolve.ClassRegistry.GetBlockEntityBehaviorClass(name) == null)
				    {
                        var pos = new BlockPos(tree.GetInt("posx", 0), tree.GetInt("posy", 0), tree.GetInt("posz", 0));
                        worldForResolve.Api.GetService<ILogger>().Warning(Lang.Get("Failed to add permanent BlockEntityBehavior {0} for {1}", name, pos));
                        continue;
				    }

                    var properties = string.IsNullOrEmpty(base64Properties) ?
                        new JsonObject(new JObject()) :
                        JsonObject.FromJson(Encoding.UTF8.GetString(Convert.FromBase64String(base64Properties)));

                    BlockEntityBehavior blockEntityBehavior = worldForResolve.ClassRegistry.CreateBlockEntityBehavior(blockEntity, name);
				    blockEntityBehavior.properties = properties;
				    blockEntity.Behaviors.Add(blockEntityBehavior);
                    
                    manager.Behaviors.Add(key, new PermanentBlockEntityBehaviorContext
                    {
                        Name = name,
                        Base64EncodedProperties = base64Properties,
                        Instance = blockEntityBehavior
                    });
                }
                catch
                {
                    var pos = new BlockPos(tree.GetInt("posx", 0), tree.GetInt("posy", 0), tree.GetInt("posz", 0));
                    worldForResolve.Api.GetService<ILogger>().Warning(Lang.Get("Failed to add permanent BlockEntityBehavior {0} for {1}", name, pos));
                }
            }
        }
    }
}
