using System;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Interfaces;

public interface IImGuiComponent
{

    public ImGuiContext Context { get; }

    public void SafeRender();
    public void Render();

    void OnError(Exception exception);
}
