using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace InsanityLib.Extended.Transitions.Patches;

[HarmonyPatch]
public static class TransitionLogicPatches
{
    [HarmonyPatch(typeof(CollectibleObject), "UpdateAndGetTransitionStatesNative")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> PreventNegativeTransitionHours(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);
        matcher.MatchEndForward(
            new CodeMatch(code =>code.opcode == OpCodes.Ldloc_S && code.operand is LocalBuilder pos && pos.LocalIndex == 18),
            new CodeMatch(() => true),
            new CodeMatch(OpCodes.Stind_R4)
        );

        matcher.InsertAndAdvance(
            new CodeInstruction(OpCodes.Ldc_R4, 0f),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Math), nameof(Math.Max), [typeof(float), typeof(float)]))
        );

        return matcher.InstructionEnumeration();
    }


    [HarmonyPatch(typeof(BarrelRecipe), nameof(BarrelRecipe.TryCraftNow))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> FixBarrelRecipeTransitionLogic(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);
        matcher.MatchStartForward(new CodeMatch(OpCodes.Stloc_2));
        
        matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TransitionLogicPatches), nameof(FilterPerishableTransitions))));

        return matcher.InstructionEnumeration();
    }

    //TODO figure out what exactly this solved and see if there is a better way to do that
    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.CarryOverFreshness), typeof(ICoreAPI), typeof(ItemSlot[]), typeof(ItemStack[]), typeof(TransitionableProperties))]
    [HarmonyPrefix]
    public static bool ReplaceCarryOverFreshness(CollectibleObject __instance, ICoreAPI api, ItemSlot[] inputSlots, ItemStack[] outStacks, TransitionableProperties perishProps)
    {
        float transitionedHoursRelative = 0f;
		float spoilageRelMax = 0f;
		float spoilageRel = 0f;
		int quantity = 0;

		foreach (ItemSlot slot in inputSlots)
		{
            TransitionState? state = slot.Itemstack?.Collectible?.UpdateAndGetTransitionState(api.World, slot, EnumTransitionType.Perish);
            if(state is null) continue;

			quantity++;

			float spoilageRelOne = Math.Max(0f, (state.TransitionedHours - state.FreshHours) / state.TransitionHours);
			spoilageRelMax = Math.Max(spoilageRelOne, spoilageRelMax);
			spoilageRel += spoilageRelOne;
            transitionedHoursRelative += state.TransitionedHours / (state.TransitionHours + state.FreshHours);
		}

		transitionedHoursRelative /= Math.Max(1, quantity);
		spoilageRel /= Math.Max(1, quantity);

        var dummySlot = new DummySlot();
        foreach (var outStack in outStacks)
        {
            if(outStack is null) continue;

            dummySlot.Itemstack = outStack;
            var transitions = outStack.Collectible?.GetTransitionableProperties(api.World, outStack, null);
            if(transitions is null) continue;
            var states = outStack.Collectible?.UpdateAndGetTransitionStates(api.World, dummySlot);
            if(states is null) continue;

            for (int transitionIndex = 0; transitionIndex < transitions.Length; transitionIndex++)
            {
                if (transitions[transitionIndex].Type != EnumTransitionType.Perish) continue;
                if (outStack.Attributes["transitionstate"] is not ITreeAttribute tree) continue;

                tree.SetDouble("createdTotalHours", api.World.Calendar.TotalHours);
                tree.SetDouble("lastUpdatedTotalHours", api.World.Calendar.TotalHours);

                var freshHoursAttr = (FloatArrayAttribute)tree["freshHours"];
                float freshHours = perishProps.FreshHours.nextFloat(1f, api.World.Rand);
                freshHoursAttr.value[transitionIndex] = freshHours;

                var transitionHoursAttr = (FloatArrayAttribute)tree["transitionHours"];
                float transitionHours = perishProps.TransitionHours.nextFloat(1f, api.World.Rand);
                transitionHoursAttr.value[transitionIndex] = transitionHours;

                var transitionedHoursAttr = (FloatArrayAttribute)tree["transitionedHours"];
                transitionedHoursAttr.value[transitionIndex] = spoilageRel > 0f ?
                    freshHours + Math.Max(0f, transitionHours * (spoilageRel * 0.6f) - 2f) :
                    Math.Max(0f, transitionedHoursRelative * (0.8f + (2 + quantity) * spoilageRelMax) * (transitionHours + freshHours));
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(BlockCrock), nameof(BlockCrock.GetDummySlotForFirstPerishableStack))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> FixDummySlotSelector(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);
        matcher.MatchStartForward(CodeMatch.Calls(AccessTools.Method(typeof(CollectibleObject), nameof(CollectibleObject.GetTransitionableProperties))));

        matcher.InsertAfterAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TransitionLogicPatches), nameof(FilterPerishableTransitions))));

        return matcher.InstructionEnumeration();
    }
    
    public static TransitionableProperties[] FilterPerishableTransitions(TransitionableProperties[] transitions) => transitions?.Where(trans => trans.Type == EnumTransitionType.Perish).ToArray();
}
