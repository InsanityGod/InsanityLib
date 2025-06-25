using InsanityLib.Attributes.Auto.Config.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Interfaces.UI.ImGuiComponents
{
    public interface IImGuiComponentContainer : IImGuiComponent, IEnumerable<IImGuiComponent>
    {
        public List<IImGuiComponent> Components { get; }
        
        /// <summary>
        /// Wether the children should be visiblke
        /// </summary>
        public bool IsDropDownOpen { get; set; }

        /// <summary>
        /// If set to false, the child components should not be rendered by the container itself
        /// </summary>
        bool ShouldRenderChildren { get; set; }
        ConfigDisplayAttribute DisplayProperties { get; set; }

        IEnumerator<IImGuiComponent> IEnumerable<IImGuiComponent>.GetEnumerator() => Components.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Renders the child components of this container.
        /// </summary>
        void RenderChildren();
    }
}
