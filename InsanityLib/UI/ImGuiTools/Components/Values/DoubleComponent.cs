using ImGuiNET;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace InsanityLib.UI.ImGuiTools.Components.Values
{
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
}
