using ImGuiNET;
using InsanityLib.Interfaces.UI.ImGui;
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

        protected virtual bool HandleError(object error) => false;

        public abstract void Render();
    }
}
