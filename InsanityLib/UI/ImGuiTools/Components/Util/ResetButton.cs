using ImGuiNET;
using InsanityLib.Attributes.Auto;
using InsanityLib.Util;
using System;
using System.ComponentModel;
using System.Reflection;

namespace InsanityLib.UI.ImGuiTools.Components.Util;

public class ResetButton : ComponentBase
{
    public readonly string DescriptionStr;
    public static ResetButton TryCreate(ImGuiContext context)
    {
        if(!context.CanWrite || context.Member.GetCustomAttribute<DefaultValueAttribute>() is null) return null;

        return new ResetButton(context.New("reset-button", name: "~"));
    }

    protected ResetButton(ImGuiContext context) : base(context)
    {
        var defaultAttr = context.Member.GetCustomAttribute<DefaultValueAttribute>();
        DescriptionStr = $"Reset to default: {(defaultAttr is AutoDefaultValueAttribute ? "RuntimeCalculated" : defaultAttr.Value)}";
    }

    public override void Render()
    {
        if (ImGui.Button(Context.Label))
        {
            try
            {
                Context.Member.SetAutoDefaultValue(Context, Context.TargetObject);
                Context.ParentContext.NotifyChanged(this); //Reset button has a sperate context, we should notify the parent context instead
            }
            catch(Exception ex)
            {
                OnError(ex);
            }
        }

        if (ImGui.BeginItemTooltip())
        {
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
            ImGui.TextUnformatted(DescriptionStr);
            ImGui.PopTextWrapPos();

            ImGui.EndTooltip();
        }

        ImGui.SameLine();
    }
}
