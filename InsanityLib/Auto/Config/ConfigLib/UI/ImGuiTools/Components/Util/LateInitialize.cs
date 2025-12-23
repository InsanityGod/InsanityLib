namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components.Util;

public class LateInitialize(ImGuiContext context) : ComponentBase(context)
{
    public override void Render()
    {
        //TODO
        //ImGui.Text($"Uninitalized Member on {Context.ComposeType.FullName}: {Context.Member?.Name}");
    }
}
