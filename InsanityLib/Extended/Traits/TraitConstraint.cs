using InsanityLib.Extended.Traits.Interfaces;
using Vintagestory.API.Common;

namespace InsanityLib.Extended.Traits;

public class TraitConstraint : ITraitSystemConstraint
{
    public required ETraitConstraintType Type { get; set; }

    public required AssetLocation Code { get; set; }

    public ETraitSystem TraitSystems { get; set; } = ETraitSystem.All;

    /// <summary>
    /// The level required or forbidden for this constraint. (level = 0 means level is irrelevant)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// From which level this constraint applies. (level = 0 means level is irrelevant)
    /// </summary>
    public int FromLevel { get; set; }
}
