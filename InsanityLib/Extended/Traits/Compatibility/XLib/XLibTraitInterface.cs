using InsanityLib.Exceptions;
using InsanityLib.Extended.Traits.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;
using XLib.XLeveling;

namespace InsanityLib.Extended.Traits.Compatibility.XLib;

internal class XLibTraitInterface(ICoreAPI api) : ITraitSystemInterface
{
    private readonly CharacterSystem characterSystem = api.ModLoader.GetModSystem<CharacterSystem>();
    private readonly XLeveling leveling = api.ModLoader.GetModSystem<XLeveling>();

    public ETraitSystem ForSystem => ETraitSystem.XLib;

    internal void TryRegisterTraits(IEnumerable<ExtendedTrait> traits)
    {
        foreach (var trait in traits)
        {
            try
            {
                RegisterTrait(trait);
            }
            catch(Exception ex)
            {
                api.Logger.Error("[InsanityLib] Failed to register ExtendedTrait '{0}' to XLib: {1}", trait.Code, ex);
            }
        }
    }

    internal void RegisterTrait(ExtendedTrait trait)
    {
        if(!trait.AllowesSystem(ETraitSystem.XLib) || trait.Skill is null || trait.MaxLevel < 1 || trait.Type == EnumTraitType.Negative) return;

        var skill = leveling.GetSkill(trait.Skill) ?? leveling.GetSkill(trait.Skill.Path);
        if(skill is null)
        {
            api.Logger.Debug("[InsanityLib] Skipped registering ExtendedTrait '{0}' to XLib: skill '{1}' does not exist", trait.Code, trait.Skill);
            return;
        }

        if(trait.MaxLevel == 1 && trait.AllowesSystem(ETraitSystem.Vanilla))
        {

            var ability = new TraitAbility(trait.Code, trait.Code);
            //ability.OnPlayerAbilityTierChanged += (PlayerAbility ability, int oldTier) => applyTraitAttributes(ability.PlayerSkill); //todo
            skill.AddAbility(ability);

            return;
        }
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "applyTraitAttributes")]
    private static extern ItemStack applyTraitAttributes(CharacterSystem instance, EntityPlayer eplr); //TODO bind on TraitSkill

    public void AddExperience(ExtendedTrait trait, float experience)
    {
        throw new System.NotImplementedException(); //TODO
    }

    public int GetEffectiveTraitLevel(ExtendedTrait trait, IPlayer player)
    {
        throw new System.NotImplementedException(); //TODO
    }
}