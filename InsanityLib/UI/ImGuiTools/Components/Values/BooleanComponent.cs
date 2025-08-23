using ImGuiNET;

namespace InsanityLib.UI.ImGuiTools.Components.Values;

public class BooleanComponent : ValueComponentBase<bool>
{
    public BooleanComponent(ImGuiContext context) : base(context)
    {
    }

    public override void RenderValue()
    {
        if(ImGui.Checkbox(Context.Label, ref value))
        {
            Context.TryAutoSetValue(value, this);
        }
    }
}
