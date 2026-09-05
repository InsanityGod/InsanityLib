using InsanityLib.Extended.Traits.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;
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
        
        string traitCode = trait.Code;
        Ability? ability = null; 
        if(trait.MaxLevel == 1 && trait.AllowesSystem(ETraitSystem.Vanilla)) //Single level traits are treated as simply gaining said trait
        {
            ability = new TraitAbility(traitCode, traitCode, trait.GetDisplayName(), trait.GetFormattedDescription());
            
            ability.OnPlayerAbilityTierChanged += static (ability, oldTier) =>
            {
                var player = ability.PlayerSkill.PlayerSkillSet.Player.Entity;
                applyTraitAttributes(player.Api.ModLoader.GetModSystem<CharacterSystem>(), player);
            };
            ability.AddRequirement(new NotRequirement(new TraitRequirement([traitCode])));
        }
        else
        {
            int statNr = 0;
            var statCodes = new string[trait.Attributes.Count];
            var values = new int[statCodes.Length * trait.MaxLevel];

            foreach((var statCode, var stat) in trait.Attributes)
            {
                statCodes[statNr] = statCode;
                for(int i = 0; i < trait.MaxLevel; i++)
                {
                    // [stat1_for_level1, stat2_for_level1, stat1_for_level2, stat2_for_level2, ...]
                    var valueNr = statNr + i * statCodes.Length;

                    if (stat.ValuePerLevel is { Length: > 0 })
                    {
                        //If the values per level don't contain enough values, continue using the last one
                        if (i < stat.ValuePerLevel.Length)
                        {
                            values[valueNr] = (int)Math.Round(stat.ValuePerLevel[i] * 100);
                        }
                        else values[valueNr] = (int)Math.Round(stat.ValuePerLevel[^1] * 100);
                    }
                    else values[valueNr] = (int)Math.Round(stat.Value * (i + 1) * 100);
                }

                statNr++;
            }

            ability = new StatsAbility(traitCode, statCodes, trait.GetDisplayName(), trait.GetUnformattedDescription(), 0, trait.MaxLevel, values: values)
            {
            };
        }

        if(ability is null) return;
        RegisterConstraints(ability, trait);
        skill.AddAbility(ability);
    }

    private void RegisterConstraints(Ability ability, ExtendedTrait trait)
    {
        //TODO
    }

    public Ability? GetAbilityForTrait(ExtendedTrait trait)
    {
        throw new NotImplementedException(); //TODO
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