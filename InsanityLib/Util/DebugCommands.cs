using InsanityLib.Auto.Command;
using InsanityLib.Auto.Command.Argument;
using InsanityLib.Auto.Config.ConfigLib.UI;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace InsanityLib.Util;

public static class DebugCommands
{
    /// <summary>
    /// Displays mod information.
    /// </summary>
    /// <param name="api"/>
    /// <param name="ModID">The ID of the mod to search for.</param>
    /// <example>/modinfo insanitylib</example>
    /// <returns>ModName (ModID ModVersion) ModDescription</returns>
    [AutoCommand(Side = EnumAppSide.Universal)]
    public static TextCommandResult ModInfo(ICoreAPI api, string ModID)
    {
        var mod = api.ModLoader.GetMod(ModID);
        if (mod is not null) return TextCommandResult.Success($"{mod.Info.Name} ({mod.Info.ModID} {mod.Info.Version})\n{mod.Info.Description}");

        var closestMatch = api.ModLoader.Mods
                .OrderBy(m => m.Info.ModID.LevenshteinDistance(ModID))
                .First();

        return TextCommandResult.Error($"No such ModID, did you mean {closestMatch.Info.ModID}?");
    }

    #if DEBUG

    //GUI tests

    [AutoCommand] public static bool IsTrue(bool value = false) => value;

    /// <summary>
    /// Opens an AutoGui for the target block.
    /// </summary>
    [AutoCommand(Path = "AutoGui", Name = "Block", RequiredPrivelege = "controlserver")]
    public static void AutoGuiForBlock(ICoreClientAPI api, [Required(ErrorMessage = "Not targeting a block")] Block block) => new AutoGuiDialog(api, block).TryOpen();

    /// <summary>
    /// Opens an AutoGui for the target item.
    /// </summary>
    [AutoCommand(Path = "AutoGui", Name = "Item", RequiredPrivelege = "controlserver")]
    public static void AutoGuiForItem(ICoreClientAPI api, [Required(ErrorMessage = "Not holding an item")] CollectibleObject collectible) => new AutoGuiDialog(api, collectible).TryOpen();

    //Github examples

    /// <summary>Will give you information about the held item stack.</summary>
    /// <example>/Debug HeldItemStack</example>
    [AutoCommand(Path = "Debug")]
    public static ItemStack HeldItemStack([Required(ErrorMessage = "Not holding any item")] ItemStack heldItem) => heldItem;

    /// <summary>Will give you information about the entity you are looking at.</summary>
    /// <example>/Debug Entity</example>
    [AutoCommand(Path = "Debug")]
    public static string Entity([Required(ErrorMessage = "Not looking at an Entity")] Entity entity) => entity.GetName();

    /// <summary>Prints a neat little message about how you think the chance of rain is like today.</summary>
    /// <example>/Forecast 25</example>
    [AutoCommand]
    public static string Forecast(ICoreAPI api, [CommandParameter(ContextualSource = EContextualSource.Caller)] IPlayer callingPlayer, [Range(0, 100)] int chance = 50) => string.Format(
        ForecastText,
        callingPlayer.PlayerName,
        chance,
        api.World.Calendar.PrettyDate()
    );

    public const string ForecastText = "{0} says that there will be a {1}% chance of rain on {2}";

    #endif
}
