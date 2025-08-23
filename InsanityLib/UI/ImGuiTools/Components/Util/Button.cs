using ImGuiNET;
using InsanityLib.Util;
using System;
using System.Numerics;

namespace InsanityLib.UI.ImGuiTools.Components.Util;

public class Button : ComponentBase
{
    public Action Action { get; set; }

    public bool FullWidth { get; set; } = true;
    public Vector2? FixedWidth { get; set; }
    public Button(ImGuiContext context, Action action = null) : base(context)
    {
        Action = action;
    }

    public void SafeExecute()
    {
        try
        {
            if(Action is not null)
            {
                Action();
            }
            else
            {
                Context.Member.AutoInvoke(Context, Context.TargetObject);
            }
        }
        catch(Exception ex)
        {
            OnError(ex);
        }
    }

    public override void Render()
    {
        if (ImGui.Button(Context.Label, FixedWidth ?? (FullWidth ? new(ImGui.GetContentRegionAvail().X, 0) : default)))
        {
            SafeExecute();
        }
    }
}
