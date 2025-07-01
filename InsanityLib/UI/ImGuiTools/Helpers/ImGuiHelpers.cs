using ImGuiNET;
using System;
using System.Numerics;
using VSImGui;

namespace InsanityLib.UI.ImGuiTools.Helpers
{
    public static class ImGuiHelpers
    {
        static public void DrawHint(ReadOnlySpan<char> hint)
        {
            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.BeginItemTooltip())
            {
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
                ImGui.TextUnformatted(hint);
                ImGui.PopTextWrapPos();

                ImGui.EndTooltip();
            }
        }

        public static void Seperator(ReadOnlySpan<char> text, ReadOnlySpan<char> description = default)
        {
            ImGui.BeginGroup();
            ImGui.Spacing();

            var style = ImGui.GetStyle();
            float contentWidth = ImGui.GetContentRegionAvail().X;
            var cursorPos = ImGui.GetCursorScreenPos();
            float lineHeight = ImGui.GetTextLineHeight();

            // Draw left separator line
            float leftLineStartX = cursorPos.X;
            float leftLineEndX = cursorPos.X + 32;
            float lineY = cursorPos.Y + lineHeight / 2.0f;
            ImGui.GetWindowDrawList().AddLine(
                new Vector2(leftLineStartX, lineY),
                new Vector2(leftLineEndX, lineY),
                ImGui.GetColorU32(ImGuiCol.Separator),
                4.0f
            );

            // Position cursor for text
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 32 + style.ItemSpacing.X);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 1);

            // Render the separator text
            ImGui.TextUnformatted(text);

            // Optionally render the description hint
            if (!description.IsEmpty) DrawHint(description);

            ImGui.SameLine();

            // Draw right separator line
            var textEndPos = ImGui.GetCursorScreenPos();
            float rightLineStartX = textEndPos.X;
            float rightLineEndX = textEndPos.X + ImGui.GetContentRegionAvail().X;
            ImGui.GetWindowDrawList().AddLine(
                new Vector2(rightLineStartX, lineY),
                new Vector2(rightLineEndX, lineY),
                ImGui.GetColorU32(ImGuiCol.Separator),
                4.0f
            );

            // Add spacing below the separator
            ImGui.NewLine();
            ImGui.Spacing();
            ImGui.EndGroup();
        }
    }
}
