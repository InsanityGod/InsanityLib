using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.AccessControl;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace InsanityLib.HarmonyPatches.LogicChanges;

[HarmonyPatch]
public static class AttachedInventoryPositionAwarenessPatches
{

    [HarmonyPatch(typeof(AttachedContainerWorkspace), nameof(AttachedContainerWorkspace.TryLoadInv))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ConnectPosition(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher.MatchStartForward(
            new CodeMatch(instruction => instruction.opcode == OpCodes.Newobj && instruction.operand is ConstructorInfo constructor && constructor.DeclaringType == typeof(InventoryGeneric))
        );

        if(matcher.IsValid)
        {
            matcher.InsertAfter(
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AttachedInventoryPositionAwarenessPatches), nameof(SetConnectionCallback)))
            );
        }

        return matcher.InstructionEnumeration();
    }

    public static void SetConnectionCallback(InventoryGeneric inventory, AttachedContainerWorkspace __instance)
    {
        //TODO maybe add a timed update as well in case the entity moves while the inventory is open
        inventory.OnInventoryOpened += (player) =>
        {
            __instance.WrapperInv.Pos = __instance.entity.SidedPos.AsBlockPos;
        };
    }
}
