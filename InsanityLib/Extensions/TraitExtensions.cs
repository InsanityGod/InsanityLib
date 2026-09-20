using InsanityLib.Util.Span;
using Vintagestory.API.Common;

namespace InsanityLib.Extensions;

public static class TraitExtensions
{
    public static int GetTraitLevel(this IPlayer player, AssetLocationSpan code)
    {
        var insanityLib = player.Entity.Api.ModLoader.GetModSystem<InsanityLibModSystem>();
        var trait = insanityLib.GetExtendedTrait(code);
        if(trait is null) return 0;
        return insanityLib.GetEffectiveTraitLevel(trait, player);
    }

    public static bool HasTrait(this IPlayer player, AssetLocationSpan code) => GetTraitLevel(player, code) > 0;

    public static void AddExperience(this IPlayer player, AssetLocationSpan code, float experience)
    {
        var insanityLib = player.Entity.Api.ModLoader.GetModSystem<InsanityLibModSystem>();
        var trait = insanityLib.GetExtendedTrait(code);
        if(trait is null) return;
        insanityLib.AddExperience(trait, player, experience);
    }
}
