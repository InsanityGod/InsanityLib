using InsanityLib.Extended.Traits.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace InsanityLib.Extended.Traits;

public sealed class VanillaTraitInterface(ICoreAPI api) : ITraitSystemInterface
{
    private readonly CharacterSystem characterSystem = api.ModLoader.GetModSystem<CharacterSystem>();

    public ETraitSystem ForSystem => ETraitSystem.Vanilla | ETraitSystem.DynamicTraits;

    public void AddExperience(ExtendedTrait trait, float experience)
    {
        //Not applicable to vanilla
    }

    public int GetEffectiveTraitLevel(ExtendedTrait trait, IPlayer player) => characterSystem.HasTrait(player, trait.Code) ? trait.LevelForTrait : 0;
}
