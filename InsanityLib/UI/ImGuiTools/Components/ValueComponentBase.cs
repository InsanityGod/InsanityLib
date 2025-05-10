using ImGuiNET;
using InsanityLib.UI.ImGuiTools.Components.Util;
using InsanityLib.Util;
using VSImGui;
using YamlDotNet.Core.Tokens;

namespace InsanityLib.UI.ImGuiTools.Components
{
    public abstract class ValueComponentBase<T> : ValueComponentBase
    {
        protected T value;
        public T Value => value;

        protected ValueComponentBase(ImGuiContext context) : base(context)
        {
            value = context.Member.GetValue(context.TargetObject).AutoConvert<T>();
        }
    }

    public abstract class ValueComponentBase : ComponentBase
    {
        protected readonly ResetButton ResetButton;

        protected ValueComponentBase(ImGuiContext context) : base(context)
        {
            ResetButton = ResetButton.TryCreate(context);
        }

        public abstract void RenderValue();

        public override void Render()
        {
            if(!Context.AllowedToWrite) ImGui.BeginDisabled();
            
            ResetButton?.SafeRender();

            try
            {
                RenderValue();
            }
            catch
            {
                //TODO logging
            }

            if(Context.Description != null) Editors.DrawHint(Context.Description);
            
            if(!Context.AllowedToWrite) ImGui.EndDisabled();
        }
    }
}
