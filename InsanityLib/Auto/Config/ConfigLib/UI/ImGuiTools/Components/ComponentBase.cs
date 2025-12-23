using ImGuiNET;
using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Interfaces;
using InsanityLib.Util.AutoRegistry;
using System;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components;

public abstract class ComponentBase(ImGuiContext context) : IImGuiComponent
{
    public object Error { get; protected set; } //TODO turn into exception class

    public ImGuiContext Context { get; private set; } = context;

    public void SafeRender()
    {
        if(Error is not null) return;

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

    public virtual void OnError(Exception exception)
    {
        if (HandleError(exception)) return;
        
        Error = exception;
        //Logging
        AutoConfigUtil.NotifyUserOfException(exception, this);
    }

    public virtual bool ContextMenuEnabled => true;

    public virtual void RenderContextMenuContent()
    {
        //Empty
    }

    public void RenderContextMenu()
    {
        if(AutoConfig.ContextMenuOwner == this)
        {
            if(!ImGui.BeginPopupContextItem()) return; //Begin the context menu popup

            AutoConfig.CurrentContextMenuClaim = this; //Set the current context menu claim

            RenderContextMenuContent(); //Render the context menu content
            ImGui.EndPopup(); //End the context menu popup
            return;
        }
        
        if (!ContextMenuEnabled || AutoConfig.ContextMenuOwner is not null && AutoConfig.ContextMenuOwner != this) return; //Context Menu is in use by another component or disabled

        if (ImGui.BeginPopupContextItem()) //Begin the context menu popup
        {
            AutoConfig.ContextMenuOwner = this;
            AutoConfig.CurrentContextMenuClaim = this; //Set the current context menu claim

            RenderContextMenuContent(); //Render the context menu content
            ImGui.EndPopup(); //End the context menu popup
        }
    }

    protected virtual bool HandleError(Exception exception) => false;

    public abstract void Render();
}
