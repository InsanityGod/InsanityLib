using InsanityLib.Algorithm;
using InsanityLib.Attributes.Auto.Command;
using InsanityLib.Behaviors.BlockEntityBehaviors;
using InsanityLib.Enums.Auto.Commands;
using InsanityLib.UI;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace InsanityLib.Commands
{
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
        public static TextCommandResult ModInfo(ICoreAPI api, [CommandParameter] string ModID)
        {
            var mod = api.ModLoader.GetMod(ModID);
            if (mod != null) return TextCommandResult.Success($"{mod.Info.Name} ({mod.Info.ModID} {mod.Info.Version})\n{mod.Info.Description}");

            var closestMatch = api.ModLoader.Mods
                    .OrderBy(m => m.Info.ModID.LevenshteinDistance(ModID))
                    .First();

            return TextCommandResult.Error($"No such ModID, did you mean {closestMatch.Info.ModID}?");
        }

        #if DEBUG

        /// <summary>
        /// Opens an AutoGui for the target block.
        /// </summary>
        [AutoCommand(RequiredPrivelege = "controlserver", Path = "insanitylib/debug/autogui", Name = "block")]
        public static void AutoGuiForBlock(ICoreClientAPI api, [CommandParameter(Source = EParamSource.CallerTarget)] [Required(ErrorMessage = "Not targeting a block")] Block block) => new AutoGuiDialog(api, block).TryOpen();

        /// <summary>
        /// Opens an AutoGui for the target item.
        /// </summary>
        [AutoCommand(RequiredPrivelege = "controlserver", Path = "insanitylib/debug/autogui", Name = "item")]
        public static void AutoGuiForItem(ICoreClientAPI api, [CommandParameter(Source = EParamSource.Caller)] CollectibleObject collectible) => new AutoGuiDialog(api, collectible).TryOpen();
        
        #endif

        /// <summary>
        /// Adds blockentity behavior to block
        /// </summary>
        [AutoCommand(RequiredPrivelege = "controlserver", Path = "insanitylib/debug/behavior", Name = "add")]
        public static bool AddPermantentBehavior(ICoreServerAPI api, [CommandParameter(Source = EParamSource.CallerTarget)] [Required(ErrorMessage = "Not targeting a valid position")] BlockPos pos, [CommandParameter(Source = EParamSource.Specify)] string behavior)
        {
            var accessor = api.World.BlockAccessor;
            var entity = accessor.GetBlockEntity(pos);
            if(entity == null)
            {
                accessor.SpawnBlockEntity("Generic", pos);
                entity = accessor.GetBlockEntity(pos);
            }

            return entity?.TryAddPermanentbehavior(behavior) != null;
        }

        /// <summary>
        /// Removes all permanent block entity behaviors from block
        /// </summary>
        [AutoCommand(RequiredPrivelege = "controlserver", Path = "insanitylib/debug/behavior", Name = "clear", Side = EnumAppSide.Server)]
        public static string ClearPermantentBehavior([CommandParameter(Source = EParamSource.CallerTarget)][Required(ErrorMessage = "Not targeting a blockentity")] BlockEntity blockEntity)
        {
            var manager = blockEntity.GetBehavior<PermanentBehaviorManager>();
            if(manager == null) return "Blockentity does not have any permanent behaviors";
            
            var toRemove = manager.Behaviors.Keys.ToList();
            foreach(var behavior in toRemove) manager.RemoveBehavior(behavior);

            return $"Removed {toRemove.Count} behaviors";
        }
    }
}
