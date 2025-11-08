using InsanityLib.UI.ImGuiTools;
using System;

namespace InsanityLib.UI.ImGuiTools.Interfaces;


public interface IImGuiComposer
{
    public bool CanComposeType(Type type);

    public IImGuiComponent Compose(ImGuiContext context, Type type);
}
