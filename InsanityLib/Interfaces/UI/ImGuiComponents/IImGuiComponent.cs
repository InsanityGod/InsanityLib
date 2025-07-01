using InsanityLib.UI.ImGuiTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Interfaces.UI.ImGuiComponents
{
    public interface IImGuiComponent
    {

        public ImGuiContext Context { get; }

        public void SafeRender();
        public void Render();

        void OnError(Exception exception);
    }
}
