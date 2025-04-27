using HarmonyLib;
using InsanityLib.JsonAssets;
using InsanityLib.Util;
using InsanityLib.Util.ContentFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace InsanityLib.HarmonyPatches.Extended
{
    [HarmonyPatch]
    public static class ExtendedTransitionPatches
    {

        [HarmonyPatch(typeof(CollectibleBehaviorHandbookTextAndExtraInfo), "addProcessesIntoInfo")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddHandbookInfo(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();
            var method = AccessTools.Method(typeof(ExtendedTransitionPatches), nameof(AddProccessIntoInfoToHandbook));

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Switch && codes[i-3].operand == AccessTools.Field(typeof(TransitionableProperties), nameof(TransitionableProperties.Type)))
                {
                    codes.InsertRange(i + 1, new CodeInstruction[]
                    {
                        new(OpCodes.Ldarg_2),
                        new(OpCodes.Ldarg_S, 5),
                        new(OpCodes.Ldloc_S, 25),
                        new(OpCodes.Ldarg_3),
                        new(OpCodes.Ldloc_S, 29),
                        new(OpCodes.Ldloca_S, 26),
                        new(OpCodes.Call, method),
                    });
                    break;
                }
            }

            return codes;
        }

        public static void AddProccessIntoInfoToHandbook(ICoreClientAPI capi, List<RichTextComponentBase> components, ClearFloatTextComponent verticalSpace, ActionConsumable<string> openDetailPageFor, TransitionableProperties prop, ref bool addedItemStack)
        {
            var handler = CustomTransition.ExtendedEnum.FindHandler(prop.Type);

            if (handler == null) return;
            addedItemStack = true;

            handler.AddProccessIntoInfoToHandbook(capi, components, verticalSpace, openDetailPageFor, prop);
        }

        [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.OnTransitionNow))]
        [HarmonyPostfix]
        public static void PostTransition(CollectibleObject __instance, ItemSlot slot, TransitionableProperties props, ref ItemStack __result)
        {
            var handler = CustomTransition.ExtendedEnum.FindHandler(props.Type);
            if (handler == null) return;

            handler.PostOnTransitionNow(__instance, slot, props, ref __result);
        }

        [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.GetTransitionRateMul))]
        [HarmonyPostfix]
        public static void GetTransitionRateMul(IWorldAccessor world, ItemSlot inSlot, EnumTransitionType transType, ref float __result)
        {
            var handler = CustomTransition.ExtendedEnum.FindHandler(transType);
            if (handler == null) return;

            __result = handler.GetTransitionRateMul(world, inSlot, __result);
        }

        [HarmonyPatch(typeof(InventoryBase), "GetDefaultTransitionSpeedMul")]
        [HarmonyPrefix]
        public static bool GetDefaultTransitionSpeedMul(EnumTransitionType transitionType, ref float __result)
        {
            var handler = CustomTransition.ExtendedEnum.FindHandler(transitionType);
            if (handler == null) return true;

            __result = handler.DefaultTransitionSpeedMul;
            return false;
        }

        [HarmonyPatch(typeof(CollectibleObject), "AppendPerishableInfoText", argumentTypes: new Type[] { typeof(ItemSlot), typeof(StringBuilder), typeof(IWorldAccessor), typeof(TransitionState) , typeof(bool) })]
        [HarmonyPrefix]
        public static bool AppendPerishableInfoText(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, TransitionState state, bool nowSpoiling)
        {
            var handler = CustomTransition.ExtendedEnum.FindHandler(state.Props.Type);
            if (handler == null) return true;

            handler.AppendAppendPerishableInfoText(inSlot, dsc, world, state, nowSpoiling);

            return false;
        }

        [HarmonyPatch(typeof(BlockLiquidContainerBase), nameof(BlockLiquidContainerBase.PerishableInfoCompact))] //TODO compact container method? (from block entity shelf)
        [HarmonyPostfix]
        public static void PerishableInfoCompact(ICoreAPI Api, ItemSlot contentSlot, ref string __result)
        {
            TransitionState[] transitionStates = contentSlot.Itemstack.Collectible.UpdateAndGetTransitionStates(Api.World, contentSlot);
            if(transitionStates == null) return;
            var builder = new StringBuilder(__result);
            builder.AppendLine();
            
            foreach(var transitionState in transitionStates)
            {
                var handler = CustomTransition.ExtendedEnum.FindHandler(transitionState.Props.Type);
                if(handler == null) continue;

                //TODO figure out what to do with nowSpoiling
                //TODO maybe a seperate method for compact info
                handler.AppendAppendPerishableInfoText(contentSlot, builder, Api.World, transitionState, false);
            }

            __result = builder.ToString();
        }
    }
}
