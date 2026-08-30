using InsanityLib.Extended.Traits;
using InsanityLib.Extended.Traits.Interfaces;
using InsanityLib.Util.Span;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;

[assembly: IgnoresAccessChecksTo("VSEssentials")]
namespace InsanityLib;

//TODO maybe a feature system (so specific features can fail "safely")
//TODO maybe add some utility for sending packets with callback logic

public partial class InsanityLibModSystem : ModSystem, ITraitSystemInterface
{
    internal Dictionary<AssetLocation, ExtendedTrait> ExtendedTraits { get; set; } = new Dictionary<AssetLocation, ExtendedTrait>(AssetLocationSpanComparer.Instance);
    
    public ExtendedTrait? GetExtendedTrait(AssetLocationSpan code) => ExtendedTraits.GetAlternateLookup<AssetLocationSpan>().TryGetValue(code, out var result) ? result : null;
    
    public ETraitSystem ForSystem => ETraitSystem.All;


    private List<ITraitSystemInterface> TraitSystems { get; set; } = [];

    public void AddExperience(ExtendedTrait trait, float experience)
    {
        //TODO
    }

    public int GetEffectiveTraitLevel(ExtendedTrait trait, IPlayer player)
    {
        int level = 0;
        foreach(var traitSystem in TraitSystems)
        {
            if((traitSystem.ForSystem & trait.AppliedSystems) == 0) continue;

            level = Math.Max(level, traitSystem.GetEffectiveTraitLevel(trait, player));
        }

        return level;
    }

    //public void GainExperience(AssetLocationSpan)

    partial void OnExtendedTraitLoaded(ICoreAPI api, AssetLocation origin, ExtendedTrait asset)
    {
        if(!asset.Enabled) return;

        if (ExtendedTraits.ContainsKey(asset.Code))
        {
            Mod.Logger.Error("Extended trait with code '{0}' already exists, ignoring.", asset.Code);
            return;
        }

        ExtendedTraits.Add(asset.Code, asset);
    }
}