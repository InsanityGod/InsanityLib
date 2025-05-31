using InsanityLib.Interfaces.UI.ImGui;
using InsanityLib.UI.ImGuiTools.Components.Util;
using InsanityLib.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.UI.ImGuiTools.Composers
{
    internal class EnumerableComposer: IImGuiComposer
    {
        public bool CanComposeType(Type type) => type.IsArray || typeof(IDictionary).IsAssignableFrom(type) || typeof(ICollection).IsAssignableFrom(type);

        public IImGuiComponent Compose(ImGuiContext context, Type type)
        {
            
            return null; //TODO
        }
    }
}
