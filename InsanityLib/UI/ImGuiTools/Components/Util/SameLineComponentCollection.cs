using ImGuiNET;
using InsanityLib.Enums.Auto.Config.UI;
using InsanityLib.Interfaces.UI.ImGui;
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

        public IImGuiComponentContainer LastActiveChildContainer { get; set; }

        public SameLineComponentCollection(ImGuiContext context) : base(context)
        {
            DisplayProperties.Hierarchy = EHierarchyDisplay.None;
        }

        //TODO late render (for dropdowns) where only 1 can be active at a time

        public override void RenderChildren()
        {
            for (var i = 0; i < Components.Count; i++)
            {
                if(i > 0) ImGui.SameLine();
                if(Spread != null && Spread.Length > i)
                {
                    var spread = Spread[i];
                    if(spread.HasValue && spread.Value != 0)
                    {
                        if(spread.Value < 0) ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X + spread.Value);
                        else ImGui.SetNextItemWidth(spread.Value);
                    }
                    else
                    {
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    }
                }
                var component = Components[i];

                if(component is IImGuiComponentContainer { DisplayProperties.Hierarchy: EHierarchyDisplay.DropDown } childContainer)
                {
                    childContainer.ShouldRenderChildren = false;
                    if(childContainer != LastActiveChildContainer) ImGui.SetNextItemOpen(false);
                    component.SafeRender();

                    if(childContainer.IsDropDownOpen && childContainer != LastActiveChildContainer)
                    {
                        LastActiveChildContainer = childContainer;
                    }
                }
                else component.SafeRender();
            }
            
            if(LastActiveChildContainer?.IsDropDownOpen == true)
            {
                var startPos = ImGui.GetCursorScreenPos();
                
                ImGui.Indent();
                LastActiveChildContainer.RenderChildren();
                ImGui.Unindent();
                
                var endPos = ImGui.GetCursorScreenPos();
                
                startPos.X += 4;
                endPos.X += 4;
                ImGui.GetWindowDrawList()
                    .AddLine(
                        startPos,
                        endPos,
                        ImGui.GetColorU32(ImGuiCol.Separator),
                        8
                    );

                //Horizontal Line
                //endPos.X -= 4;
                //endPos.Y += 4;
                //var endPos2 = endPos with
                //{
                //    X = endPos.X + ImGui.GetContentRegionAvail().X
                //};

                //ImGui.GetWindowDrawList()
                //    .AddLine(
                //        endPos,
                //        endPos2,
                //        ImGui.GetColorU32(ImGuiCol.Separator),
                //        8
                //    );

                ImGui.NewLine();
            }
        }
    }
}
