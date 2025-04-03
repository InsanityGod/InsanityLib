using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Attributes.Auto
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DisposalLogicAttribute : Attribute
    {
        public DisposalLogicAttribute(int priority) => Priority = priority;
        public readonly int Priority;
    }
}
