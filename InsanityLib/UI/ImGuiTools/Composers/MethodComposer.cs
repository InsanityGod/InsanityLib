using InsanityLib.Attributes.Auto.Config.UI;
using InsanityLib.Interfaces.UI.ImGuiComponents;
using InsanityLib.UI.ImGuiTools.Components.Util;
using System;
using System.Reflection;

namespace InsanityLib.UI.ImGuiTools.Composers;

public class MethodComposer : IImGuiComposer
{
    public bool CanComposeType(Type type) => typeof(MethodBase).IsAssignableFrom(type);

    public IImGuiComponent Compose(ImGuiContext context, Type type)
    {
        if(context.Member is not MethodBase || context.Member.GetCustomAttribute<ConfigMethodAttribute>() is null) return null;
        return new Button(context);
    }
}
