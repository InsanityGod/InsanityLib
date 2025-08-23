namespace InsanityLib.UI.ImGuiTools.Components.Util;

public class LateInitialize : ComponentBase
{
    public LateInitialize(ImGuiContext context) : base(context)
    {

    }

    public override void Render()
    {
        //TODO
        //ImGui.Text($"Uninitalized Member on {Context.ComposeType.FullName}: {Context.Member?.Name}");
    }
}
