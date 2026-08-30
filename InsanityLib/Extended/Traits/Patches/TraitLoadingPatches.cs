using HarmonyLib;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace InsanityLib.Extended.Traits.Patches;

[HarmonyPatch]
public static class TraitLoadingPatches
{

    [HarmonyPatch(typeof(CharacterSystem), "LoadTraits")]
    [HarmonyPostfix]
    public static void AppendExtendedTraits(ICoreAPI ___api, List<Trait> ___traits)
    {
        var insanityLib = ___api.ModLoader.GetModSystem<InsanityLibModSystem>();

        foreach(var trait in insanityLib.ExtendedTraits.Values.ForSystem(ETraitSystem.Vanilla))
        {
            ___traits.Add(trait.AsVanillaTrait(___api));
        }
    }

    [HarmonyPatch(typeof(CharacterSystem), "loadCharacterClasses")]
    [HarmonyPostfix]
    public static void ModifyClasses(ICoreAPI ___api, CharacterSystem __instance)
    {
        var insanityLib = ___api.ModLoader.GetModSystem<InsanityLibModSystem>();
        
        foreach(var trait in insanityLib.ExtendedTraits.Values)
        {
            if(trait.AppendToClasses is null || (trait.AppliedSystems & ETraitSystem.Vanilla) == 0) continue;
            //TODO AppendToRaces/Species
            string traitAsString = trait.Code;

            foreach(var targetClass in trait.AppendToClasses)
            {
                if(__instance.characterClassesByCode.TryGetValue(targetClass, out var characterClass))
                {
                    //TODO maybe optimize array re-creation logic
                    characterClass.Traits = [..characterClass.Traits, traitAsString];
                }
            }
        }
    }
}
