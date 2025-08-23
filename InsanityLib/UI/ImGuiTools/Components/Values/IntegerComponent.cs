using ImGuiNET;

namespace InsanityLib.UI.ImGuiTools.Components.Values;

public class IntegerComponent : ValueComponentBase<int>
{
    public IntegerComponent(ImGuiContext context) : base(context)
    {
    }

    public override void RenderValue()
    {
        if(ImGui.DragInt(Context.Label, ref value))
        {
            Context.TryAutoSetValue(value, this);
        }
    }
}
