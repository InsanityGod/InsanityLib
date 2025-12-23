using ImGuiNET;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components.Values;

public class BooleanComponent(ImGuiContext context) : ValueComponentBase<bool>(context)
{
    public override void RenderValue()
    {
        if(ImGui.Checkbox(Context.Label, ref value))
        {
            Context.TryAutoSetValue(value, this);
        }
    }
}
