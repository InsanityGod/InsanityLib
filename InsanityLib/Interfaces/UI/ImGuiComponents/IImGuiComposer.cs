using InsanityLib.UI.ImGuiTools;
using System;

namespace InsanityLib.Interfaces.UI.ImGuiComponents;


public interface IImGuiComposer
{
    public bool CanComposeType(Type type);

    public IImGuiComponent Compose(ImGuiContext context, Type type);
}
