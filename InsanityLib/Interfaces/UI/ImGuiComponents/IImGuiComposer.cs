using InsanityLib.UI.ImGuiTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Interfaces.UI.ImGuiComponents
{

    public interface IImGuiComposer
    {
        public bool CanComposeType(Type type);

        public IImGuiComponent Compose(ImGuiContext context, Type type);
    }
}
