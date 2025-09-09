using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace InsanityLib.Util;

public static class HarmonyUtil
{
    public static CodeMatch CodeMatchConstructor(this Type type) => new(instruction => instruction.IsConstructorFor(type));

    public static bool IsConstructorFor(this CodeInstruction instruction, Type type) => instruction.opcode == OpCodes.Newobj && instruction.operand is ConstructorInfo ctor && ctor.DeclaringType == type;

    public static CodeMatch CodeMatchMethodReturnType(this Type type) => new(instruction => instruction.IsMethodReturning(type));

    public static bool IsMethodReturning(this CodeInstruction instruction, Type type) => 
        (instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt) 
        && instruction.operand is MethodInfo method && type.IsAssignableFrom(method.ReturnType);

    public static CodeMatch CodeMatchStoresLocal(this Type type) => new(instruction => instruction.IsStoringLocal(type));

    public static bool IsStoringLocal(this CodeInstruction instruction, Type type) => instruction.IsStloc() && instruction.operand is LocalBuilder builder && type.IsAssignableFrom(builder.LocalType);

    public static int GetStoredLocalIndex(this CodeInstruction instruction)
    {
        if(!instruction.IsStloc()) throw new ArgumentException("passed instruction is not for storing locals", nameof(instruction));
        if(instruction.operand is LocalBuilder builder) return builder.LocalIndex;
        return (int)instruction.operand;
    }
}
