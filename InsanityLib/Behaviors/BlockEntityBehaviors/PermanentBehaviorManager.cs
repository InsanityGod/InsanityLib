using InsanityLib.Contexts;
using InsanityLib.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace InsanityLib.Behaviors.BlockEntityBehaviors
{
    public class PermanentBehaviorManager : BlockEntityBehavior, IEnumerable<BlockEntityBehavior>
    {
        public PermanentBehaviorManager(BlockEntity blockentity) : base(blockentity)
        {
        }

        internal readonly Dictionary<string, PermanentBlockEntityBehaviorContext> Behaviors = new();

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            var permanentBehaviorsTree = tree.GetOrAddTreeAttribute("permanent-behaviors");

            foreach((var key, var behavior) in Behaviors)
            {
                var behaviorTree = new TreeAttribute();
                behaviorTree.SetString("name", behavior.Name);
                behaviorTree.SetString("properties", behavior.Base64EncodedProperties);

                permanentBehaviorsTree[key] = behaviorTree;
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            //TODO this should be injected in defaultFromTreeAttributes

            var permanentBehaviorsTree = tree.GetTreeAttribute("permanent-behaviors");
            if (permanentBehaviorsTree == null) return;

            // Remove behaviors that no longer exist in the tree
            var keysToRemove = Behaviors.Keys.Where(key => !permanentBehaviorsTree.HasAttribute(key)).ToList();
            foreach (var key in keysToRemove)
            {
                var behavior = Behaviors[key].Instance;
                Behaviors.Remove(key);
                Blockentity.Behaviors.Remove(behavior);
                //TODO maybe have an interface for PermanentBehavior Events
            }

            // Add or update behaviors that exist in the tree but not in the dictionary
            foreach ((var key, var attr) in permanentBehaviorsTree)
            {
                if (Behaviors.ContainsKey(key) || attr is not TreeAttribute behaviorTree) continue;

                var name = behaviorTree.GetString("name");
                var base64Properties = behaviorTree.GetString("properties");

                try
                {
                    if (worldAccessForResolve.ClassRegistry.GetBlockEntityBehaviorClass(name) == null)
                    {
                        var pos = Blockentity.Pos;
                        worldAccessForResolve.Api.GetService<ILogger>().Warning(Lang.Get("Failed to add permanent BlockEntityBehavior {0} for {1}", name, pos));
                        continue;
                    }

                    var properties = string.IsNullOrEmpty(base64Properties) ?
                        new JsonObject(new JObject()) :
                        JsonObject.FromJson(Encoding.UTF8.GetString(Convert.FromBase64String(base64Properties)));

                    var blockEntityBehavior = worldAccessForResolve.ClassRegistry.CreateBlockEntityBehavior(Blockentity, name);
                    blockEntityBehavior.properties = properties;
                    Behaviors.Add(key, new PermanentBlockEntityBehaviorContext
                    {
                        Name = name,
                        Base64EncodedProperties = base64Properties,
                        Instance = blockEntityBehavior
                    });
                    Blockentity.Behaviors.Add(blockEntityBehavior);
                }
                catch
                {
                    var pos = Blockentity.Pos;
                    worldAccessForResolve.Api.GetService<ILogger>().Warning(Lang.Get("Failed to add permanent BlockEntityBehavior {0} for {1}", name, pos));
                }
            }
        }

        //TODO add
        //TODO remove

        public IEnumerator<BlockEntityBehavior> GetEnumerator() => Behaviors.Values.Select(context => context.Instance).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
