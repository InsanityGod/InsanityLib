using DynamicClassesModSystem;
using HarmonyLib;
using InsanityLib.Util;
using InsanityLib.Util.Span;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Client.NoObf;

namespace InsanityLib.Extended.Traits.Compatibility.DynamicTraits.Patches;

[HarmonyPatch]
[HarmonyPatchCategory("feature:extendedtraits_dynamictraits")]
internal static class DynamicTraitsTraitLanguagePatch
{
    [HarmonyPatch("DynamicClassesModSystem.DynamicClassesModSystem", nameof(ModSystem.Start))]
    [HarmonyPostfix]
    internal static void Postfix(ModSystem __instance, ICoreAPI api)
    {
        if(api.Side != EnumAppSide.Client) return;

        var channel = (NetworkChannel)((DynamicClassesModSystem.DynamicClassesModSystem)__instance).clientCh!;
        channel.AfterPacket<TraitsCatalogPacket>(_ => FixupTraits(api));
    }

    internal static void FixupTraits(ICoreAPI api)
    {
        var extendedTraitLookup = api.ModLoader.GetModSystem<InsanityLibModSystem>().ExtendedTraits.GetAlternateLookup<AssetLocationSpan>();
        foreach (var trait in DynamicClassesModSystem.DynamicClassesModSystem.clientTraits.Values)
        {
            if(!extendedTraitLookup.TryGetValue(trait.Code, out var extendedTrait)) continue;
            trait.Title = Lang.Get(LangUtil.ConcatKeyWithDomainSupport("traittitle-", trait.Code));

            StringBuilder sb = new();
            string str;

            var descriptionKey = LangUtil.ConcatKeyWithDomainSupport("traitdesc-", trait.Code);
            str = Lang.Get(descriptionKey);
            if(str != descriptionKey && !string.IsNullOrEmpty(str)) sb.AppendLine(str);
            
            var attributes = trait.Attrs;
            if(attributes.Count == 0 && extendedTrait.Attributes.Count > 0)
            {
                attributes = extendedTrait.GetAttributesForVanilla(api);
            }
            bool firstEntry = true;
            foreach((var key, var attr) in attributes)
            {
                if (!firstEntry)
                {
                    sb.Append(", ");
                }
                else firstEntry = false;
                //TODO add language strings for base game attributes
                var langKey = $"charattribute-{key}"; //TODO domain support for custom attributes/stats
                str =  Lang.Get(langKey, attr);

                if(str != langKey && !string.IsNullOrEmpty(str)) sb.Append(str);
            }

            trait.Description = sb.ToString();
        }

    }
}
