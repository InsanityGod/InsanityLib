using ImGuiNET;
using InsanityLib.Enums.Auto.Config.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.UI.ImGuiTools.Components.Util
{
    public class SameLineComponentCollection : ComponentCollection //TODO maybe make table component instead
    {
        public int?[] Spread { get; set;}
        public SameLineComponentCollection(ImGuiContext context) : base(context)
        {
            DisplayProperties.Hierarchy = EHierarchyDisplay.None;
        }

        //TODO late render (for dropdowns) where only 1 can be active at a time

        protected override void RenderChildren()
        {
            for(var i = 0; i < Components.Count; i++)
            {
                if(i > 0) ImGui.SameLine();
                if(Spread != null && Spread.Length > i)
                {
                    var spread = Spread[i];
                    if(spread.HasValue) ImGui.SetNextItemWidth(spread.Value);
                    else if(i == Components.Count - 1) //TODO be able to reserve space after the empty spot
                    {
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    }
                }
                Components[i].SafeRender();
            }
        }
    }
}
