using InsanityLib.UI.ImGuiTools;
using System;

namespace InsanityLib.Interfaces.UI.ImGuiComponents;

public interface IImGuiComponent
{

    public ImGuiContext Context { get; }

    public void SafeRender();
    public void Render();

    void OnError(Exception exception);
}
