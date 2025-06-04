using ImGuiNET;
using InsanityLib.Attributes.Auto.Config.UI;
using InsanityLib.Enums.Auto.Config.UI;
using InsanityLib.Interfaces.UI.ImGui;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSImGui;

namespace InsanityLib.UI.ImGuiTools.Components.Util
{
    public class ComponentCollection : ComponentBase, IImGuiComponentContainer
    {
        public ConfigDisplayAttribute DisplayProperties { get; set; }

        public ComponentCollection(ImGuiContext context) : base(context)
        {
            DisplayProperties = context.Member.GetCustomAttribute<ConfigDisplayAttribute>() ?? new();
        }

        public IList<IImGuiComponent> Components { get; set; } = new List<IImGuiComponent>();

        public override void Render()
        {
            switch (DisplayProperties.Hierarchy)
            {
                case EHierarchyDisplay.DropDown:
                    DropDown();
                    break;

                default:
                    RenderChildren();
                    break;
            }
        }

        protected virtual void RenderChildren()
        {
            for(var i = 0; i < Components.Count; i++)
            {
                Components[i].SafeRender();
            }
        }

        private void DropDown()
        {
            var open = ImGui.CollapsingHeader(Context.Label);
            if(Context.Description != null) Editors.DrawHint(Context.Description);
            
            if (!open) return;
            
            ImGui.Indent();
            
            RenderChildren(); //TODO Group Displacement
            
            ImGui.Unindent();
        }
    }
}
