using HarmonyLib;
using InsanityLib.Util;
using InsanityLib.Util.ContentFeatures;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace InsanityLib.HarmonyPatches.Extended;

[HarmonyPatch]
public static class ExtendedTransitionPatches
{

    [HarmonyPatch(typeof(CollectibleBehaviorHandbookTextAndExtraInfo), "addProcessesIntoInfo")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> AddHandbookInfo(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);
        
        matcher.MatchEndForward
        (
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TransitionableProperties), nameof(TransitionableProperties.Type))),
            new CodeMatch(),
            new CodeMatch(),
            new CodeMatch(OpCodes.Switch)
        );
        var switchLoc = matcher.Pos;
        
        matcher.MatchEndBackwards(typeof(ClearFloatTextComponent).CodeMatchStoresLocal());
        var verticalSpaceLocalIndex = matcher.Instruction.GetStoredLocalIndex();

        matcher.MatchEndForward(typeof(bool).CodeMatchStoresLocal());
        var addedItemStackLocalIndex = matcher.Instruction.GetStoredLocalIndex();

        matcher.MatchEndForward(typeof(TransitionableProperties).CodeMatchStoresLocal());
        var propsLocalIndex = matcher.Instruction.GetStoredLocalIndex();

        matcher.Start();
        matcher.Advance(switchLoc);
        matcher.InsertAfter(
            CodeInstruction.LoadArgument(1), //capi
            CodeInstruction.LoadArgument(4), //components
            CodeInstruction.LoadLocal(verticalSpaceLocalIndex), //verticalSpace
            CodeInstruction.LoadArgument(2), //openDetailPageFor
            CodeInstruction.LoadLocal(propsLocalIndex), //props
            CodeInstruction.LoadLocal(addedItemStackLocalIndex, true), //addedItemStack
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtendedTransitionPatches), nameof(AddProccessIntoInfoToHandbook)))
        );

        return matcher.InstructionEnumeration();
    }

    public static void AddProccessIntoInfoToHandbook(ICoreClientAPI capi, List<RichTextComponentBase> components, ClearFloatTextComponent verticalSpace, ActionConsumable<string> openDetailPageFor, TransitionableProperties prop, ref bool addedItemStack)
    {
        var handler = CustomTransition.ExtendedEnum.FindHandler(prop.Type);

        if (handler is null) return;
        addedItemStack = true;

        handler.AddProccessIntoInfoToHandbook(capi, components, verticalSpace, openDetailPageFor, prop);
    }

    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.OnTransitionNow))]
    [HarmonyPostfix]
    public static void PostTransition(CollectibleObject __instance, ItemSlot slot, TransitionableProperties props, ref ItemStack __result)
    {
        var handler = CustomTransition.ExtendedEnum.FindHandler(props.Type);
        if (handler is null) return;

        handler.PostOnTransitionNow(__instance, slot, props, ref __result);
    }

    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.GetTransitionRateMul))]
    [HarmonyPostfix]
    public static void GetTransitionRateMul(IWorldAccessor world, ItemSlot inSlot, EnumTransitionType transType, ref float __result)
    {
        var handler = CustomTransition.ExtendedEnum.FindHandler(transType);
        if (handler is null) return;

        __result = handler.GetTransitionRateMul(world, inSlot, __result);
    }

    [HarmonyPatch(typeof(InventoryBase), "GetDefaultTransitionSpeedMul")]
    [HarmonyPrefix]
    public static bool GetDefaultTransitionSpeedMul(EnumTransitionType transitionType, ref float __result)
    {
        var handler = CustomTransition.ExtendedEnum.FindHandler(transitionType);
        if (handler is null) return true;

        __result = handler.DefaultTransitionSpeedMul;
        return false;
    }

    [HarmonyPatch(typeof(CollectibleObject), "AppendPerishableInfoText", argumentTypes: new Type[] { typeof(ItemSlot), typeof(StringBuilder), typeof(IWorldAccessor), typeof(TransitionState) , typeof(bool) })]
    [HarmonyPrefix]
    public static bool AppendPerishableInfoText(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, TransitionState state, bool nowSpoiling)
    {
        var handler = CustomTransition.ExtendedEnum.FindHandler(state.Props.Type);
        if (handler is null) return true;

        handler.AppendAppendPerishableInfoText(inSlot, dsc, world, state, nowSpoiling);

        return false;
    }

    [HarmonyPatch(typeof(BlockLiquidContainerBase), nameof(BlockLiquidContainerBase.PerishableInfoCompact))] //TODO compact container method? (from block entity shelf)
    [HarmonyPostfix]
    public static void PerishableInfoCompact(ICoreAPI Api, ItemSlot contentSlot, ref string __result)
    {
        TransitionState[] transitionStates = contentSlot.Itemstack.Collectible.UpdateAndGetTransitionStates(Api.World, contentSlot);
        if(transitionStates is null) return;
        var builder = new StringBuilder(__result);
        builder.AppendLine();
        
        foreach(var transitionState in transitionStates)
        {
            var handler = CustomTransition.ExtendedEnum.FindHandler(transitionState.Props.Type);
            if(handler is null) continue;

            //TODO figure out what to do with nowSpoiling
            //TODO maybe a seperate method for compact info
            handler.AppendAppendPerishableInfoText(contentSlot, builder, Api.World, transitionState, false);
        }

        __result = builder.ToString();
    }
}
