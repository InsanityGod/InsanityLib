using ImGuiNET;
using InsanityLib.Util;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Reflection;
using System.Xml.Linq;
using VSImGui;

namespace InsanityLib.UI.ImGuiTools.Components.Values
{
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
}
