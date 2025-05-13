using HarmonyLib;
using InsanityLib.Util;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace InsanityLib.HarmonyPatches.LogicChanges
{

    [HarmonyPatch]
    public static class PermanentBlockEntitybehaviorPatches
    {
        [HarmonyPatch(typeof(BlockEntity), nameof(BlockEntity.FromTreeAttributes))]
        [HarmonyPrefix]
        public static void PatchFromTreeAttributes(BlockEntity __instance, ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            var permanentBehaviorTree = tree.GetTreeAttribute("permanent-behaviors");
            if (permanentBehaviorTree == null) return;
            __instance.GetOrCreatePermanentBehaviorManager().UpdateBehaviorsFromTree(tree, worldAccessForResolve);
        }
    }
}
