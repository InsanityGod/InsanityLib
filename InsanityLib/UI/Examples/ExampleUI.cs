#if DEBUG
using System.ComponentModel;
using Vintagestory.API.Client;

namespace InsanityLib.UI.Examples
{
    [DisplayName("Example Title")]
    public class ExampleUI
    {
        public string UseCase => "Simple Display & Input";
    
        public string DesiredItem { get; set; } = "Apples";
        public int DesiredAmount { get; set; } = 1;
    
        public void ShowChatMessage(ICoreClientAPI api) => api.SendChatMessage($"{api.World.Player.PlayerName} wishes he had {DesiredAmount} {DesiredItem}");
    }
}
#endif