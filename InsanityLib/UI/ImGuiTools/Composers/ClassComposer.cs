using InsanityLib.Interfaces.UI.ImGui;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.UI.ImGuiTools.Composers
{
    public class ClassComposer : IImGuiComposer
    {
        public bool CanComposeType(Type type) => type.IsComplexClassType();

        public IImGuiComponent Compose(ImGuiContext context, Type type)
        {
            //TODO
            throw new NotImplementedException();
        }
    }
}
