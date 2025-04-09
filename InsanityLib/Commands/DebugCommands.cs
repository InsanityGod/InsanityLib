using InsanityLib.Algorithm;
using InsanityLib.Attributes.Auto.Command;
using InsanityLib.Enums.Auto.Commands;
using InsanityLib.Util.AutoRegistry;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace InsanityLib.Commands
{
    public static class DebugCommands
    {
        [AutoCommand(Side = EnumAppSide.Universal)]
        public static TextCommandResult ModInfo(ICoreAPI api, [CommandParameter] string ModID)
        {
            //TODO maybe make a mod arg parser
            var mod = api.ModLoader.GetMod(ModID);
            if (mod != null) return TextCommandResult.Success($"{mod.Info.Name} ({mod.Info.ModID} {mod.Info.Version})\n{mod.Info.Description}");

            //TODO maybe have a better algorithm
            var closestMatch = api.ModLoader.Mods
                    .OrderBy(m => m.Info.ModID.LevenshteinDistance(ModID)) 
                    .First();

            return TextCommandResult.Error($"No such ModID, did you mean {closestMatch.Info.ModID}?");
        }

        [AutoCommand(RequiredPrivelege = "controlserver")]
        public static void ApplyGravity(
            IWorldAccessor world,
            [CommandParameter(Source = EParamSource.CallerTarget)] [Required(ErrorMessage = "Target is not UnstableFalling")] BlockBehaviorUnstableFalling beh,
            [CommandParameter(Source = EParamSource.CallerTarget)] BlockPos pos)
        {
            var handling = EnumHandling.PassThrough;
            beh?.OnBlockPlaced(world, pos, ref handling);
        }
        
        #if DEBUG
        
        [AutoCommand(RequiredPrivelege = "controlserver", Path = "AutoGui", Name = "Block")]
        public static void AutoGuiForBlock(ICoreClientAPI api, [CommandParameter(Source = EParamSource.CallerTarget)] Block block) => api.OpenAutoGui(block);

        [AutoCommand(RequiredPrivelege = "controlserver", Path = "AutoGui", Name = "Item")]
        public static void AutoGuiForItem(ICoreClientAPI api, [CommandParameter(Source = EParamSource.Caller)] Item item) => api.OpenAutoGui(item);
        
        #endif
    }
}
