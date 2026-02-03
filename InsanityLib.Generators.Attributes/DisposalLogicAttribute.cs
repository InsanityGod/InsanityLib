using System.Diagnostics;
using Vintagestory.API.Common;

namespace InsanityLib.Generators.Attributes;

/// <summary>
/// Used to mark methods that should be run when the game is doing disposal logic.
/// </summary>
[Conditional("CompileTimeOnly")]
[AttributeUsage(AttributeTargets.Method)]
public sealed class DisposalLogicAttribute : Attribute
{
    /// <summary>
    /// Represents the order in which things have to be disposed (Higher numbers run first)
    /// </summary>
    public int ExecutionOrder { get; set; }

    /// <summary>
    /// The side on which the disposal logic should run. <br />
    /// If set to <see cref="EnumAppSide.Universal"/>, the logic is allowed to run on either side but will only run on twice if <see cref="MayRunTwice"/> is set to true.
    /// </summary>
    public EnumAppSide Side { get; set; } = EnumAppSide.Universal;

    /// <summary>
    /// Wether the logic is allowed to run twice
    /// </summary>
    public bool MayRunTwice { get; set; }
}
