using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Interfaces.UI.ImGui
{
    public interface IImGuiComponentContainer : IImGuiComponent, IEnumerable<IImGuiComponent>
    {
        public IList<IImGuiComponent> Components { get; }

        IEnumerator<IImGuiComponent> IEnumerable<IImGuiComponent>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
