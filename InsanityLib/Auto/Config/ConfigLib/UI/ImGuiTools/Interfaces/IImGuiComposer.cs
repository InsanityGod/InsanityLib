using System;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Interfaces;


public interface IImGuiComposer
{
    public bool CanComposeType(Type type);

    public IImGuiComponent? Compose(ImGuiContext context, Type type);
}
