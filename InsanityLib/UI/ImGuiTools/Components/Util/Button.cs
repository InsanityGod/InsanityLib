using ImGuiNET;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSImGui;

namespace InsanityLib.UI.ImGuiTools.Components.Util
{
    public class Button : ComponentBase
    {
        public Action Action { get; set; }
        public string LastError { get; private set; }
        public bool FullWidth { get; set; } = true;
        public Vector2? FixedWidth { get; set; }
        public Button(ImGuiContext context, Action action = null) : base(context)
        {
            Action = action;
        }

        public string SafeExecute()
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
                return ex.ToString();
            }
            return null;
        }

        public override void Render()
        {
            ImGui.BeginDisabled(LastError is not null || !Context.CanWrite);

            if (ImGui.Button(Context.Label, FixedWidth ?? (FullWidth ? new(ImGui.GetContentRegionAvail().X, 0) : default)))
            {
                LastError = SafeExecute();
                //TODO maybe an error popup
            }

            ImGui.EndDisabled();
        }
    }
}
