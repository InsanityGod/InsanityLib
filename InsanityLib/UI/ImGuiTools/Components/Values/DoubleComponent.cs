using ImGuiNET;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace InsanityLib.UI.ImGuiTools.Components.Values;

public class DoubleComponent : ValueComponentBase<double>
{
    public string FormatString { get; set;}

    public DoubleComponent(ImGuiContext context) : base(context)
    {
        FormatString = context.Member.GetCustomAttribute<DisplayFormatAttribute>()?.DataFormatString;
    }

    public override void RenderValue()
    {
        if(ImGui.InputDouble(Context.Label, ref value, 0, 0, FormatString))
        {
            Context.TryAutoSetValue(value, this);
        }
    }
}
