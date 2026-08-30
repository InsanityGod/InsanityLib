using DynamicClassesModSystem;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Vintagestory.API.Common;

namespace InsanityLib.Extended.Traits.Compatibility.DynamicTraits.Patches;

[HarmonyPatch]
[HarmonyPatchCategory("feature:extendedtraits_dynamictraits")]
public static class DynamicTraitsTraitLoadingPatch
{
    [HarmonyPatch("DynamicClassesModSystem.DynamicClassesModSystem", "LoadOrCreateTraitsConfig")]
    [HarmonyPostfix]
    public static void Postfix(ModSystem __instance, ICoreAPI api, object __result)
    {
        var insanityLib = api.ModLoader.GetModSystem<InsanityLibModSystem>();
        try
        {
            AddExtendedTraits(insanityLib, __result);
        }
        catch(Exception ex)
        {
            insanityLib.Mod.Logger.Error("Something went wrong during dynamic traits compatibility, exception: {0}", ex);
        }
    }

    //TODO think of a good way to handle configuration
    private static void AddExtendedTraits(InsanityLibModSystem insanityLib, object traitsDictAsObj)
    {
        if(traitsDictAsObj is not Dictionary<string, TraitInfo> loadedTraits) return;

        var traitsByDomain = insanityLib.ExtendedTraits.Values
            .ForSystem(ETraitSystem.DynamicTraits)
            .GroupBy(static extendedTrait => extendedTrait.Code.Domain)
            .ToDictionary(static group => group.Key, group => group.Where(item => item.DynamicTraitCost is not null).ToArray());

        foreach((var _, var traits) in traitsByDomain)
        {
            foreach(var trait in traits)
            {
                var points = -trait.DynamicTraitCost!.Value;
                var newEntry = loadedTraits[trait.Code] = new()
                {
                    Code = trait.Code,
                    Incompat = [..trait.Constraints.ForSystem(ETraitSystem.DynamicTraits).Where(constraint => constraint.Type == ETraitConstraintType.ForbiddenTrait).Select(constraint => constraint.Code)],
                    Points = points,
                    TypeOrd = points < 0 ? 1 : 0
                };

                if(!trait.AllowesSystem(ETraitSystem.Vanilla))
                {
                    newEntry.Attrs = trait.GetAttributesForVanilla(insanityLib._api!);
                }

                trait.AppliedSystems |= ETraitSystem.DynamicTraits;
            }

        }
    }
}
