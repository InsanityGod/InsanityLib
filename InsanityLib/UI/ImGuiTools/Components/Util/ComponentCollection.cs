using InsanityLib.Interfaces.UI.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.UI.ImGuiTools.Components.Util
{
    public class ComponentCollection : ComponentBase, IImGuiComponentContainer
    {
        public ComponentCollection(ImGuiContext context) : base(context) { }

        public ICollection<IImGuiComponent> Components { get; set; } = new List<IImGuiComponent>();

        public override void Render()
        {
            foreach(var component in Components)
            {
                component.SafeRender();
            }
        }
    }
}
