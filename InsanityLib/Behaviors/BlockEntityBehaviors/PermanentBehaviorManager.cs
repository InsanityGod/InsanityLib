using InsanityLib.Contexts;
using InsanityLib.Interfaces;
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
        public const string PermanentBehaviorTreeKey = "permanent-behaviors";
        public PermanentBehaviorManager(BlockEntity blockentity) : base(blockentity)
        {
        }

        internal readonly Dictionary<string, PermanentBlockEntityBehaviorContext> Behaviors = new();

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            var permanentBehaviorsTree = tree.GetOrAddTreeAttribute(PermanentBehaviorTreeKey);

            foreach((var key, var behavior) in Behaviors)
            {
                var behaviorTree = new TreeAttribute();
                behaviorTree.SetString("name", behavior.Name);
                behaviorTree.SetString("properties", behavior.Base64EncodedProperties);

                permanentBehaviorsTree[key] = behaviorTree;
            }
        }

        public string GetId(BlockEntityBehavior permanentBehavior) => Behaviors
                .Where(pair => pair.Value.Instance == permanentBehavior)
                .Select(static pair => pair.Key)
                .FirstOrDefault();

        public bool RemoveBehavior(BlockEntityBehavior behavior) => RemoveBehavior(GetId(behavior));

        public bool RemoveBehavior(string key) => Api.Side == EnumAppSide.Server && Remove(key);

        private bool Remove(string key)
        {
            if(!Behaviors.TryGetValue(key, out var context)) return false;
            var blockEntityBehavior = context.Instance;

            if(blockEntityBehavior == null) return false;
            if(blockEntityBehavior is IPermanentBehavior permanentBehavior) permanentBehavior.OnRuntimeRemoved();
            if(blockEntityBehavior is IDisposable disposable) disposable.Dispose();
            Behaviors.Remove(key);
            Blockentity.Behaviors.Remove(blockEntityBehavior);
            Blockentity.MarkDirty();
            return true;
        }
        
        public void UpdateBehaviorsFromTree(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            var permanentBehaviorsTree = tree.GetTreeAttribute(PermanentBehaviorTreeKey);
            if (permanentBehaviorsTree == null) return;

            // Remove behaviors that no longer exist in the tree
            var keysToRemove = Behaviors.Keys.Where(key => !permanentBehaviorsTree.HasAttribute(key)).ToList();
            foreach (var key in keysToRemove) Remove(key);

            // Add behaviors that exist in the tree but not in the dictionary
            foreach ((var key, var attr) in permanentBehaviorsTree)
            {
                if (Behaviors.ContainsKey(key) || attr is not TreeAttribute behaviorTree) continue;

                var name = behaviorTree.GetString("name");
                var base64Properties = behaviorTree.GetString("properties");

                try
                {
                    if (worldAccessForResolve.ClassRegistry.GetBlockEntityBehaviorClass(name) == null)
                    {
                        worldAccessForResolve.Api.GetService<ILogger>().Warning(Lang.Get("Failed to add permanent BlockEntityBehavior {0} for {1}", name, Blockentity.Pos));
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
                    blockEntityBehavior.properties = properties;

                    //If the BlockEntity doesn't have the API yet then that means we are loading the world (and this will get called automatically later)
                    if (Blockentity.Api != null)
                    {
                        blockEntityBehavior.Initialize(worldAccessForResolve.Api, properties);
                        if(blockEntityBehavior is IPermanentBehavior permanentBehavior) permanentBehavior.OnRuntimeAdded();
                    }
                }
                catch(Exception ex)
                {
                    worldAccessForResolve.Api.GetService<ILogger>().Error(Lang.Get("Failed to add permanent BlockEntityBehavior {0} for {1}, ex: {2}", name, Blockentity.Pos, ex));
                }
            }
        }

        public IEnumerator<BlockEntityBehavior> GetEnumerator() => Behaviors.Values.Select(context => context.Instance).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
