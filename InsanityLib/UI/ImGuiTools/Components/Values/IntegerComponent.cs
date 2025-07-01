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
}
