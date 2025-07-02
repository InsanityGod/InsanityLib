using ImGuiNET;
using InsanityLib.Attributes.Auto.Config.UI;
using InsanityLib.Config.Util;
using InsanityLib.Enums.Auto.Config.UI;
using InsanityLib.Interfaces.UI.ImGuiComponents;
using InsanityLib.UI.ImGuiTools.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.GameContent;
using VSImGui;

namespace InsanityLib.UI.ImGuiTools.Components.Util
{
    public class ComponentCollection : ComponentBase, IImGuiComponentContainer
    {
        public ConfigDisplayAttribute DisplayProperties { get; set; }

        public bool HideDescription { get; set; }
        public string LabelOverride { get; set; }
        public bool Spacing { get; set; }

        public ComponentCollection(ImGuiContext context) : base(context)
        {
            DisplayProperties = context.Member?.GetCustomAttribute<ConfigDisplayAttribute>() ?? new();
        }

        public List<IImGuiComponent> Components { get; set; } = new List<IImGuiComponent>();

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
            if(Spacing) ImGui.Spacing(); //TODO maybe make spacing size configureable
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
            IsDropDownOpen = ImGui.CollapsingHeader(LabelOverride ?? Context.Label);
            RenderContextMenu();

            if(Context.Description is not null) Editors.DrawHint(Context.Description);
            
            if (!IsDropDownOpen || !ShouldRenderChildren) return;
            
            ImGui.Indent();
            
            RenderChildren();
            
            ImGui.Unindent();
        }

        private void Seperator()
        {
            ImGuiHelpers.Seperator(LabelOverride ?? Context.Text,  HideDescription ? default : Context.Description);
            
            RenderContextMenu();
            
            if (!ShouldRenderChildren) return;
            
            RenderChildren();
        }
    }
}
