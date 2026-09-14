using InsanityLib.Extended.Traits.Interfaces;
using Vintagestory.API.Common;

namespace InsanityLib.Extended.Traits;

public class TraitConstraint : ITraitSystemConstraint
{
    public required ETraitConstraintType Type { get; set; }

    public ETraitSystem TraitSystems { get; set; } = ETraitSystem.All;

    /// <summary>
    /// The skill required or forbidden for this constrant.
    /// Mutual Exclusive with <see cref="TraitCode"/>
    /// </summary>
    public AssetLocation? Skill { get; set; }

    /// <summary>
    /// The trait code required or forbidden for this constrant
    /// Mutual Exclusive with <see cref="Skill"/>
    /// </summary>
    public required AssetLocation? TraitCode { get; set; }

    /// <summary>
    /// The level required or forbidden for this constraint. (level = 0 means level is irrelevant)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// From which level this constraint applies. (level = 0 means level is irrelevant)
    /// </summary>
    public int FromLevel { get; set; }

    /// <summary>
    /// Whether this constraint is active
    /// </summary>
    public bool Enabled { get; set;}
}
