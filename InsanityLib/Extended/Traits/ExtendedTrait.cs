using InsanityLib.Extended.Traits.Interfaces;
using InsanityLib.Generators.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace InsanityLib.Extended.Traits;

[AssetCategory("extendedtraits", true, EnumAppSide.Universal)]
public class ExtendedTrait : ITraitSystemConstraint
{

    /// <summary>
    /// Identifier of the trait
    /// </summary>
    [Required]
    public required AssetLocation Code { get; set; }

    public EnumTraitType Type { get; set; } = EnumTraitType.Positive;


    /// <summary>
    /// Allows for specifying special types of traits that can be used for special handling in other systems (like XSkills specializations)
    /// </summary>
    public ETraitSystem TraitSystems { get; set; } = ETraitSystem.All; //TODO maybe have exlucivity / priority on what system to use?

    /// <summary>
    /// The trait systems which this trait has been registered to
    /// </summary>
    [JsonIgnore]
    [ReadOnly(true)]
    public ETraitSystem AppliedSystems { get; internal set; }

    /// <summary>
    /// Whether the trait should be registered
    /// </summary>
    public bool Enabled { get; set; } = true;

    public Dictionary<string, TraitAttribute> Attributes { get; set; } = [];

    public TraitConstraint[] Constraints { get; set; } = [];

    /// <summary>
    /// Trait will be automatically added to these classes
    /// </summary>
    public string[] AppendToClasses { get; set; } = [];

    //Mod Compatibility values:

    /// <summary>
    /// The skill to register this as an ability under: "domain:skill" (required to be set if you want it to register to XLib)
    /// example: "game:survival-metalworking" (note: XSkills does not use domains so it's considered "game" in regards to domain) //TODO deal with weirdness of group names in XSkills
    /// </summary>
    public AssetLocation? Skill { get; set; }

    /// <summary>
    /// Whether this should be a specialization. (custom logic for XSkills for instance)
    /// </summary>
    public bool IsSpecialization { get; set; } = false;

    /// <summary>
    /// The maximum level of the trait (used to make this a multi level XSkills ability)
    /// </summary>
    public int MaxLevel { get; set; } = 1;

    /// <summary>
    /// Which level of the trait vanilla and similar systems should use when calculating the value of the trait.
    /// </summary>
    public int LevelForTrait { get; set; } = 1;

    /// <summary>
    /// The points for Sonito's Dynamic Traits. (required to be set if you want it to register to dynamic traits)
    /// </summary>
    public int? DynamicTraitCost { get; set; }

    public Trait AsVanillaTrait(ICoreAPI api) => new()
    {
        Code = Code,
        Type = Type,
        Attributes = GetAttributesForVanilla(api)
    };

    public Dictionary<string, double> GetAttributesForVanilla(ICoreAPI api) => Attributes.ToDictionary(pair => pair.Key, pair =>
    {
        if(pair.Value.ValuePerLevel is not null)
        {
            if(LevelForTrait < 1 || LevelForTrait > pair.Value.ValuePerLevel.Length)
            {
                api.Logger.Error("[InsanityLib] Trait {0} has a LevelForTrait of {1} but the ValuePerLevel array has a length of {2}. Using default Value instead.", Code, LevelForTrait, pair.Value.ValuePerLevel.Length);
                return pair.Value.Value;
            }

            return pair.Value.ValuePerLevel[LevelForTrait - 1];
        }

        return pair.Value.Value;
    });

    //TODO method for checking if it's applied to player (should return the level)
    //TODO method for gaining experience
}
