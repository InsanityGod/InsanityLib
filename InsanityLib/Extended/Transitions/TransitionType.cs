using InsanityLib.Extended.AssetCategories;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace InsanityLib.Extended.Transitions;

[AssetCategory("transitiontypes", true, EnumAppSide.Universal)]
public class TransitionType
{
    /// <summary>
    /// The code for this transition
    /// </summary>
    [Required]
    public AssetLocation Code { get; set; }

    /// <summary>
    /// The handler to use for the transition
    /// </summary>
    [DefaultValue("insanitylib:transitionhandler")]
    public AssetLocation Handler { get; set; } = "insanitylib:transitionhandler";

    /// <summary>
    /// Attributes for the transition handler
    /// </summary>
    [JsonConverter(typeof(JsonAttributesConverter))]
    public JsonObject Attributes { get; set; }
}
