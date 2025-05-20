using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Interfaces
{
    public interface IPermanentBehavior
    {
        public void OnRuntimeAdded()
        {
            //Optional
        }

        public void OnRuntimeRemoved()
        {
            //Optional
        }
    }
}
