using ImGuiNET;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components.Values;

public class IntegerComponent(ImGuiContext context) : ValueComponentBase<int>(context)
{
    public override void RenderValue()
    {
        if(ImGui.DragInt(Context.Label, ref value))
        {
            Context.TryAutoSetValue(value, this);
        }
    }
}
