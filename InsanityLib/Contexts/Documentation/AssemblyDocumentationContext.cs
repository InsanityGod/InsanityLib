using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace InsanityLib.Contexts.Documentation
{
    public class AssemblyDocumentationContext
    {
        public XmlDocument Document { get; internal set; }

        public bool HasXmlDocumentation => Document is not null;
    }
}
