using ImGuiNET;
using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Interfaces;
using System;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components;

public abstract class ComponentBase(ImGuiContext context) : IImGuiComponent
{
    public object? Error { get; protected set; }

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
                Error = deepEx;
            }
        }
    }

    public virtual void OnError(Exception exception)
    {
        if (HandleError(exception)) return;
        
        Error = exception;
        //Logging
        AutoConfigLib.NotifyUserOfException(exception, this);
    }

    public virtual bool ContextMenuEnabled => true;

    public virtual void RenderContextMenuContent()
    {
        //Empty
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
        
        if (!ContextMenuEnabled || AutoConfigLib.ContextMenuOwner is not null && AutoConfigLib.ContextMenuOwner != this) return; //Context Menu is in use by another component or disabled

        if (ImGui.BeginPopupContextItem()) //Begin the context menu popup
        {
            AutoConfigLib.ContextMenuOwner = this;
            AutoConfigLib.CurrentContextMenuClaim = this; //Set the current context menu claim

            RenderContextMenuContent(); //Render the context menu content
            ImGui.EndPopup(); //End the context menu popup
        }
    }

    protected virtual bool HandleError(Exception exception) => false;

    public abstract void Render();
}
