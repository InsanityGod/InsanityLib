using InsanityLib.Enums.Auto.Config.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Attributes.Auto.Config.UI
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigDisplayAttribute : Attribute
    {
        public EHierarchyDisplay Hierarchy { get; set; }
    }
}
