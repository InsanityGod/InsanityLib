using ImGuiNET;
using InsanityLib.Config.Util;
using InsanityLib.Interfaces.UI.ImGuiComponents;
using System;

namespace InsanityLib.UI.ImGuiTools.Components
{
    public abstract class ComponentBase : IImGuiComponent
    {
        public object Error { get; protected set; }
        
        public ImGuiContext Context { get; private set; }

        protected ComponentBase(ImGuiContext context) => Context = context;

        public void SafeRender()
        {
            if(Error != null) return; //TODO maybe a way to make it still show up but disabled instead

            try
            {
                Render();
            }
            catch(Exception ex)
            {
                try
                {
                    OnError(ex);
                }
                catch (Exception deepEx) 
                {
                    //Just in case of faulty error handling in custom implementations
                    Error = deepEx; //TODO have this include the original exception
                }
            }
        }

        public virtual void OnError(object error)
        {
            if (HandleError(error)) return;
            
            Error = error;
            //Logging
        }

        public virtual bool ContextMenuEnabled => true;

        public virtual void RenderContextMenuContent()
        {

        }

        public void RenderContextMenu()
        {
            if(AutoConfigLib.ContextMenuOwner == this)
            {
                if(!ImGui.BeginPopupContextItem()) return; //Begin the context menu popup

                AutoConfigLib.CurrentContextMenuClaim = this; //Set the current context menu claim

                RenderContextMenuContent(); //Render the context menu content
                ImGui.EndPopup(); //End the context menu popup
                return;
            }
            
            if (!ContextMenuEnabled || (AutoConfigLib.ContextMenuOwner != null && AutoConfigLib.ContextMenuOwner != this)) return; //Context Menu is in use by another component or disabled

            if (ImGui.BeginPopupContextItem()) //Begin the context menu popup
            {
                AutoConfigLib.ContextMenuOwner = this;
                AutoConfigLib.CurrentContextMenuClaim = this; //Set the current context menu claim

                RenderContextMenuContent(); //Render the context menu content
                ImGui.EndPopup(); //End the context menu popup
            }

            //TODO global context menu items (like reload)
        }

        protected virtual bool HandleError(object error) => false;

        public abstract void Render();
    }
}
