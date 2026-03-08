using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace InsanityLib.Extended.Transitions.Patches;

[HarmonyPatch]
internal static class DisplayPatches
{
    //TODO CreatedBy!
    [HarmonyPatch(typeof(CollectibleBehaviorHandbookTextAndExtraInfo), "addProcessesIntoInfo")]
    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> AddProcessesIntoInfoPatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator).Start();
        
        matcher.MatchStartForward
        (
            CodeMatch.LoadsConstant("handbook-processesinto-transition-"),
            new CodeMatch(),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(TransitionableProperties), nameof(TransitionableProperties.Type)))
        );

        var startPos = matcher.Pos;
        matcher.MatchEndForward(
            //new CodeMatch(OpCodes.Ldloc_S, 60) refuses to work for who knows what reason
            new CodeMatch(instruction => instruction.opcode == OpCodes.Ldloc_S && instruction.operand is LocalBuilder l && l.LocalIndex == 60)
        );
        matcher.RemoveInstructionsInRange(startPos, matcher.Pos - 1);
        matcher.Start().Advance(startPos + 1);
        matcher.RemoveInstruction();
        matcher.Advance(-1);
        
        matcher.InsertAndAdvance(
            CodeInstruction.LoadArgument(1), //capi
            CodeInstruction.LoadLocal(58) //prop
        );
        
        matcher.MatchEndForward(
            new CodeMatch(instruction => instruction.operand is MethodInfo { Name: "Get", DeclaringType.Name: "Lang" })
        );
        matcher.Instruction.opcode = OpCodes.Call;
        matcher.Instruction.operand = AccessTools.Method(typeof(DisplayPatches), nameof(GetProcessIntoStringForHandbook));

        return matcher.InstructionEnumeration();
    }

    internal static string GetProcessIntoStringForHandbook(ICoreClientAPI capi, TransitionableProperties prop, string displayTime, params object[] extra)
    {
        var handler = CustomTransition.ExtendedEnum.FindHandler(prop.Type);
        if(handler is not null)
        {
            return handler.GetProcessIntoStringForHandbook(capi, prop, displayTime, extra);
        }
        return Lang.Get("handbook-processesinto-transition-" + prop.Type.ToString().ToLowerInvariant() + displayTime, extra);
    }

    [HarmonyPatch(typeof(CollectibleBehaviorHandbookTextAndExtraInfo), "addCreatedByInfo")]
    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> AddCreatedByInfoPatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator).Start();
        
        matcher.MatchStartForward
        (
            CodeMatch.LoadsConstant("handbook-createdby-transition-")
        );

        var startPos = matcher.Pos;
        matcher.MatchEndForward(
            new CodeMatch(instruction => instruction.operand is MethodInfo { Name: "Concat", DeclaringType.Name: "String" })
        );
        matcher.RemoveInstructionsInRange(startPos + 3, matcher.Pos);
        matcher.Start().Advance(startPos);
        matcher.RemoveInstruction();
        matcher.InsertAndAdvance(
            CodeInstruction.LoadArgument(1) //capi
        );
        matcher.Advance(1);
        matcher.InsertAfterAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DisplayPatches), nameof(GetCreatedByLangKeyForHandbook)))
        );

        return matcher.InstructionEnumeration();
    }

    internal static string GetCreatedByLangKeyForHandbook(ICoreClientAPI capi, EnumTransitionType type)
    {
        var handler = CustomTransition.ExtendedEnum.FindHandler(type);
        if(handler is not null)
        {
            return handler.GetCreatedByLangKeyForHandbook(capi, type);
        }
        return "handbook-createdby-transition-" + type.ToString().ToLowerInvariant();
    }



    [HarmonyPatch(typeof(CollectibleObject), "AppendPerishableInfoText", typeof(ItemSlot), typeof(StringBuilder), typeof(IWorldAccessor), typeof(TransitionState) , typeof(bool))]
    [HarmonyPrefix]
    internal static bool AppendPerishableInfoText(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, TransitionState state, bool nowSpoiling)
    {
        var handler = CustomTransition.ExtendedEnum.FindHandler(state.Props.Type);
        if (handler is null) return true;

        handler.AppendAppendPerishableInfoText(inSlot, dsc, world, state, nowSpoiling);

        return false;
    }

    [HarmonyPatch(typeof(BlockLiquidContainerBase), nameof(BlockLiquidContainerBase.PerishableInfoCompact))] //TODO compact container method? (from block entity shelf)
    [HarmonyPostfix]
    internal static void PerishableInfoCompact(ICoreAPI Api, ItemSlot contentSlot, ref string __result)
    {
        TransitionState[] transitionStates = contentSlot.Itemstack.Collectible.UpdateAndGetTransitionStates(Api.World, contentSlot);
        if(transitionStates is null) return;
        var builder = new StringBuilder(__result);
        builder.AppendLine();

        //Linux has a weird issue where the first character gets cut off when appending text (see issue #1 on BrainFreeze), this is a workaround for that.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) builder.Append(' ');
        
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
