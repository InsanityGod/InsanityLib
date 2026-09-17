using InsanityLib.Extended.Traits.Interfaces;
using Vintagestory.API.Common;

namespace InsanityLib.Extended.Traits;

public class TraitConstraint : ITraitSystemConstraint
{
    public required ETraitConstraintType Type { get; set; }

    public ETraitSystem TraitSystems { get; set; } = ETraitSystem.All;

    /// <summary>
    /// The skill required or forbidden for this constrant.
    /// </summary>
    public AssetLocation? Skill { get; set; }

    /// <summary>
    /// The trait code required or forbidden for this constrant.
    /// If targeting a ability that is not a Extended Trait make sure to configure <see cref="Skill"/>
    /// </summary>
    public required AssetLocation? TraitCode { get; set; }

    /// <summary>
    /// The level required or forbidden for this constraint.
    /// If not specified and targeting <see cref="TraitCode"/>, <see cref="ExtendedTrait.LevelForTrait"/> will be used otherwise defaults to 1
    /// </summary>
    public int? Level { get; set; }

    /// <summary>
    /// From which level this constraint applies. (level = 0 means level is irrelevant)
    /// </summary>
    public int FromLevel { get; set; }

    /// <summary>
    /// Whether this constraint is active
    /// </summary>
    public bool Enabled { get; set; } = true;
}
