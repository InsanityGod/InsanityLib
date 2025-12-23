using ImGuiNET;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components.Values;

public class DoubleComponent(ImGuiContext context) : ValueComponentBase<double>(context)
{
    public string FormatString { get; set; } = context.Member.GetCustomAttribute<DisplayFormatAttribute>()?.DataFormatString;

    public override void RenderValue()
    {
        if(ImGui.InputDouble(Context.Label, ref value, 0, 0, FormatString))
        {
            Context.TryAutoSetValue(value, this);
        }
    }
}
