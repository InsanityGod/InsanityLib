using InsanityLib.Generators.Attributes;
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
    public required AssetLocation Code { get; init; }

    /// <summary>
    /// The handler to use for the transition
    /// </summary>
    [DefaultValue("insanitylib:transitionhandler")]
    public AssetLocation Handler { get; init; } = "insanitylib:transitionhandler";

    /// <summary>
    /// Attributes for the transition handler
    /// </summary>
    [JsonConverter(typeof(JsonAttributesConverter))]
    public JsonObject? Attributes { get; init; }
}
