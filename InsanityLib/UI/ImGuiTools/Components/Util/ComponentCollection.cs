using ImGuiNET;
using InsanityLib.Attributes.Auto.Config.UI;
using InsanityLib.Config.Util;
using InsanityLib.Enums.Auto.Config.UI;
using InsanityLib.Interfaces.UI.ImGuiComponents;
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
            DisplayProperties = context.Member?.GetCustomAttribute<ConfigDisplayAttribute>() ?? new();
        }

        public IList<IImGuiComponent> Components { get; set; } = new List<IImGuiComponent>();

        public bool IsDropDownOpen { get; set; }

        public bool ShouldRenderChildren { get; set; } = true;

        public override void Render()
        {
            switch (DisplayProperties.Hierarchy)
            {
                case EHierarchyDisplay.DropDown:
                    DropDown();
                    break;

                case EHierarchyDisplay.Seperator:
                    Seperator();
                    break;

                default:
                    if(!ShouldRenderChildren) return;
                    RenderChildren();
                    break;
            }
        }

        public virtual void RenderChildren()
        {
            ImGui.BeginGroup();
            for(var i = 0; i < Components.Count; i++)
            {
                Components[i].SafeRender();
            }
            ImGui.EndGroup();
        }

        public override bool ContextMenuEnabled => ShouldRenderChildren;

        public override void RenderContextMenuContent()
        {
            ImGui.SeparatorText("Hierarchy Display Options");
            if(DisplayProperties.Hierarchy != EHierarchyDisplay.DropDown && ImGui.MenuItem("Collapse Content"))
            {
                DisplayProperties.Hierarchy = EHierarchyDisplay.DropDown;
            }

            if(DisplayProperties.Hierarchy != EHierarchyDisplay.Seperator && ImGui.MenuItem("Seperate Content"))
            {
                DisplayProperties.Hierarchy = EHierarchyDisplay.Seperator;
            }
        }

        private void DropDown()
        {
            IsDropDownOpen = ImGui.CollapsingHeader(Context.Label);
            RenderContextMenu();

            if(Context.Description != null) Editors.DrawHint(Context.Description);
            
            if (!IsDropDownOpen || !ShouldRenderChildren) return;
            
            ImGui.Indent();
            
            RenderChildren();
            
            ImGui.Unindent();
        }

        private void Seperator()
        {
            ImGuiHelpers.Seperator(Context.Text, Context.Description);
            
            RenderContextMenu();
            
            if (!ShouldRenderChildren) return;
            
            RenderChildren();
            ImGui.NewLine();
        }
    }
}
