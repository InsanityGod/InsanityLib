using ImGuiNET;
using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Interfaces;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components.Util;

public class Popup(ImGuiContext context) : ComponentBase(context), IImGuiComponentContainer
{
    public string? Title { get; set; }

    public string? Text { get; set; }

    public Action? AcceptCallback { get; set; }
    public string? AcceptLabel { get; set; }

    public Action? RejectCallback { get; set; }
    public string? RejectLabel { get; set; }
    public Vector4? TextColor { get; set; }

    public List<IImGuiComponent> Components { get; init; } = [];

    public bool IsDropDownOpen { get; set; } = true;
    public bool ShouldRenderChildren { get; set; } = true;
    public ConfigDisplayAttribute DisplayProperties { get; set; } = new ConfigDisplayAttribute
    {
        Hierarchy = EHierarchyDisplay.None,
    };

    public bool IsOpen => _IsOpen;
    private bool _IsOpen = true;

    public override void Render()
    {
        if (!IsOpen) return;

        var viewport = ImGui.GetMainViewport();
        var minSize = new Vector2(viewport.Size.X * 0.25f, viewport.Size.Y * 0.25f);
        ImGui.SetNextWindowSizeConstraints(minSize, minSize * 2);
        
        var label = $"{Title}##Popup-{Context.Id}";
        ImGui.OpenPopup(label);
        if (!ImGui.BeginPopupModal(label, ref _IsOpen, ImGuiWindowFlags.AlwaysAutoResize)) return;

        // Text (if any)
        if (!string.IsNullOrEmpty(Text))
        {
            if (TextColor is not null) ImGui.TextColored(TextColor.Value, Text);
            else ImGui.Text(Text);
            ImGui.Spacing();
        }

        // ChildComponents
        if (ShouldRenderChildren)
        {
            RenderChildren();
            ImGui.Spacing();
        }

        ImGui.Dummy(new Vector2(0, ImGui.GetContentRegionAvail().Y - ImGui.GetFrameHeight() - ImGui.GetStyle().ItemSpacing.Y));
        ImGui.SetCursorPosY(ImGui.GetWindowHeight() - ImGui.GetFrameHeight() - ImGui.GetStyle().WindowPadding.Y);

        // Accept/Reject buttons
        bool acceptButton = AcceptCallback is not null || !string.IsNullOrEmpty(AcceptLabel);
        bool rejectButton = RejectCallback is not null || !string.IsNullOrEmpty(RejectLabel);

        float buttonWidth = acceptButton && rejectButton ? (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2f : ImGui.GetContentRegionAvail().X;

        if (acceptButton && ImGui.Button(AcceptLabel ?? $"Accept##Popup-Accept-{Context.Id}", size: new Vector2(buttonWidth, 0)))
        {
            AcceptCallback?.Invoke();
            _IsOpen = false;
            ImGui.CloseCurrentPopup();
        }

        if (rejectButton)
        {
            if(acceptButton) ImGui.SameLine();
            if(ImGui.Button(RejectLabel ?? $"Cancel##Popup-Cancel-{Context.Id}", size: new Vector2(buttonWidth, 0)))
            {
                RejectCallback?.Invoke();
                _IsOpen = false;
                ImGui.CloseCurrentPopup();
            }
        }

        ImGui.EndPopup();
    }

    public void RenderChildren()
    {
        foreach(var component in Components)  component.SafeRender();
    }
}
