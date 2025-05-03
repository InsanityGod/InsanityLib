using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Interfaces.UI.ImGui
{
    public interface IImGuiComponentContainer : IImGuiComponent
    {
        public ICollection<IImGuiComponent> Components { get; }
    }
}
