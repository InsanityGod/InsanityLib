using InsanityLib.Attributes.Auto.Config.UI;
using InsanityLib.Interfaces.UI.ImGui;
using InsanityLib.UI.ImGuiTools.Components.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.UI.ImGuiTools.Composers
{
    public class MethodComposer : IImGuiComposer
    {
        public bool CanComposeType(Type type) => typeof(MethodBase).IsAssignableFrom(type);

        public IImGuiComponent Compose(ImGuiContext context, Type type)
        {
            if(context.Member is not MethodBase || context.Member.GetCustomAttribute<ConfigMethodAttribute>() == null) return null;
            return new Button(context);
        }
    }
}
