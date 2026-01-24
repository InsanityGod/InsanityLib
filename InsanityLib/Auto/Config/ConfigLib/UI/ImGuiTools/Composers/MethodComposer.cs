using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components.Util;
using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Interfaces;
using System;
using System.Reflection;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Composers;

public class MethodComposer : IImGuiComposer
{
    public bool CanComposeType(Type type) => typeof(MethodBase).IsAssignableFrom(type);

    public IImGuiComponent? Compose(ImGuiContext context, Type type)
    {
        if(context.Member is not MethodBase || context.Member.GetCustomAttribute<ConfigMethodAttribute>() is null) return null;
        return new Button(context);
    }
}
