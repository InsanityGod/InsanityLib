using InsanityLib.Contexts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace InsanityLib.Behaviors.BlockEntityBehaviors
{
    public class PermanentBehaviorManager : BlockEntityBehavior, IEnumerable<BlockEntityBehavior> //TODO turn into ICollectionInstead
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
            //TODO check for ID's that no longer exist and remove those behaviors
            //TODO check for ID's that didn't exist yet and add them
        }

        //TODO add
        //TODO remove

        public IEnumerator<BlockEntityBehavior> GetEnumerator() => Behaviors.Values.Select(context => context.Instance).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
