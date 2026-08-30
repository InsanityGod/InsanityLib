using Vintagestory.API.Common;

namespace InsanityLib.Extended.Traits.Interfaces;

public interface ITraitSystemInterface
{
    public ETraitSystem ForSystem { get; }

    public bool HasTrait(ExtendedTrait trait, IPlayer player) => GetEffectiveTraitLevel(trait, player) > 0;

    public int GetEffectiveTraitLevel(ExtendedTrait trait, IPlayer player);

    public void AddExperience(ExtendedTrait trait, float experience);
}
