using ImGuiNET;
using InsanityLib.Interfaces.UI.ImGui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSImGui;

namespace InsanityLib.UI.ImGuiTools.Components.Util
{
    public class DropDown : ComponentBase
    {
        public ComponentCollection ComponentCollection { get; }

        protected DropDown(ComponentCollection componentCollection) : base(componentCollection.Context.New("collapse")) => ComponentCollection = componentCollection;

        public override void Render()
        {
            if (!ImGui.CollapsingHeader(Context.Label))
            {
                if(Context.Description != null) Editors.DrawHint(Context.Description);
                return;
            }
            
            ImGui.Indent();
            
            ComponentCollection.SafeRender(); //TODO Group Displacement
            
            ImGui.Unindent();
        }
    }
}
