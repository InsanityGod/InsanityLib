using InsanityLib.Extended.Traits.Interfaces;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using XLib.XLeveling;

namespace InsanityLib.Extended.Traits.Compatibility.XLib;

internal class XLibTraitInterface(ICoreAPI api) : ITraitSystemInterface
{
    private readonly XLeveling leveling = api.ModLoader.GetModSystem<XLeveling>();
    private readonly InsanityLibModSystem insanityLib = api.ModLoader.GetModSystem<InsanityLibModSystem>();
    
    public ETraitSystem ForSystem => ETraitSystem.XLib;


    internal void TryRegisterTraits(IEnumerable<ExtendedTrait> traits)
    {
        Dictionary<ExtendedTrait,Ability> abilities = [];
        foreach (var trait in traits)
        {
            try
            {
                var ability = RegisterTrait(trait);
                if(ability is not null) abilities.Add(trait, ability);
                trait.AppliedSystems |= ETraitSystem.XLib;
            }
            catch(Exception ex)
            {
                api.Logger.Error("[InsanityLib] Failed to register ExtendedTrait '{0}' to XLib: {1}", trait.Code, ex);
            }
        }

        foreach ((var trait, var ability) in abilities)
        {
            try
            {
                RegisterConstraints(ability, trait);
            }
            catch(Exception ex)
            {
                api.Logger.Error("[InsanityLib] Failed to register constraints for ExtendedTrait XLib Ability '{0}', exception: {1}", ability.Name, ex);
            }
        }
    }
    private Skill? FindSkill(AssetLocation? skill) => skill is null ? null : leveling.GetSkill(skill) ?? leveling.GetSkill(skill.Path);
    internal Ability? RegisterTrait(ExtendedTrait trait)
    {
        if (!trait.AllowesSystem(ETraitSystem.XLib) || trait.Skill is null || trait.MaxLevel < 1 || trait.Type == EnumTraitType.Negative) return null;

        var skill = FindSkill(trait.Skill);
        if (skill is null)
        {
            api.Logger.Debug("[InsanityLib] Skipped registering ExtendedTrait '{0}' to XLib: skill '{1}' does not exist", trait.Code, trait.Skill);
            return null;
        }

        string traitCode = trait.Code;
        Ability? ability;
        if (trait.Attributes.Count > 0)
        {
            int statNr = 0;
            var statCodes = new string[trait.Attributes.Count];
            var values = new int[statCodes.Length * trait.MaxLevel];

            foreach ((var statCode, var stat) in trait.Attributes)
            {
                statCodes[statNr] = statCode;
                for (int i = 0; i < trait.MaxLevel; i++)
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

            ability = new StatsAbility(traitCode, statCodes, trait.GetDisplayName(), trait.GetUnformattedDescription(), 0, trait.MaxLevel, values: values);
        }
        else if (trait.MaxLevel == 1 && trait.AllowesSystem(ETraitSystem.Vanilla)) //Single level traits without attributes are treated as simply gaining said trait
        {
            ability = new TraitAbility(traitCode, traitCode, trait.GetDisplayName(), trait.GetFormattedDescription());
        }
        else
        {
            ability = new Ability(traitCode, trait.GetDisplayName(), trait.GetUnformattedDescription(), 0, trait.MaxLevel);
        }

        if (ability is null) return null;
        skill.AddAbility(ability);

        if (trait.IsSpecialization)
        {
            if (skill.SpecialisationID != -1)
            {
                api.Logger.Error("[InsanityLib] Failed to assign '{0}' as specialization on '{1}' as specialization is already set to '{2}'", ability.Name, skill.Name, skill.Ability(skill.SpecialisationID).Name);
            }
            else skill.SpecialisationID = ability.Id;
        }

        return ability;
    }

    private void RegisterConstraints(Ability ability, ExtendedTrait trait)
    {
        if(trait.MaxLevel == trait.LevelForTrait)
        {
            ability.AddRequirement(new NotRequirement(new TraitRequirement([ability.Name])));
        }

        foreach(var constraint in trait.Constraints)
        {
            if(!constraint.Enabled) return;
            if (constraint.TraitCode is not null)
            {
                AddRequirement(ability, constraint, GetTraitRequirement(constraint));
            }
            else if(constraint.Skill is not null)
            {
                AddRequirement(ability, constraint, GetSkillRequirement(constraint));
            }
        }
    }

    private Requirement? GetTraitRequirement(TraitConstraint constraint)
    {
        if(constraint.TraitCode is not { } traitCode) return null;
        var extendedTrait = insanityLib.GetExtendedTrait(traitCode);

        if(FindSkill(extendedTrait?.Skill ?? constraint.Skill) is { } skill && skill.FindAbility(constraint.TraitCode) is { } ability)
        {
            Requirement requirement = new AbilityRequirement(ability, constraint.Level ?? extendedTrait?.LevelForTrait ?? 1, constraint.FromLevel);
            if(ability is TraitAbility traitAbility)
            {
                requirement = new OrRequirement(requirement, new TraitRequirement([traitAbility.Trait], constraint.FromLevel));
            }
            return requirement;
        }

        if (extendedTrait == null || extendedTrait.AllowesSystem(ETraitSystem.Vanilla)) return new TraitRequirement([traitCode.Domain == "game" ? traitCode.Path : traitCode], constraint.FromLevel);
        
        return null;
    }

    private SkillRequirement? GetSkillRequirement(TraitConstraint constraint)
    {
        var skill = FindSkill(constraint.Skill);
        if(skill is null) return null;
        return new SkillRequirement(skill, constraint.Level ?? 1, constraint.FromLevel);
    }

    private static void AddRequirement(Ability ability, TraitConstraint constraint, Requirement? requirement)
    {
        if (requirement is null) return;

        if (constraint.Type == ETraitConstraintType.Forbidden) requirement = new NotRequirement(requirement);
        ability.AddRequirement(requirement);
    }

    public void AddExperience(ExtendedTrait trait, IPlayer player, float experience)
    {
        PlayerSkill? skill = FindPlayerSkill(trait, player);

        if (skill is null) return;
        skill.AddExperience(experience);
    }

    private PlayerSkill? FindPlayerSkill(ExtendedTrait trait, IPlayer player)
    {
        var skillset = leveling.IXLevelingAPI.GetPlayerSkillSet(player);
        if(trait.Skill is null) return null;
        var skill = skillset.FindSkill(trait.Skill) ?? skillset.FindSkill(trait.Skill.Path);
        return skill;
    }

    public int GetEffectiveTraitLevel(ExtendedTrait trait, IPlayer player) => FindPlayerSkill(trait, player)?.FindAbility(trait.Code)?.Tier ?? 0;

}