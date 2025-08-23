using System.Xml;

namespace InsanityLib.Contexts.Documentation;

public class AssemblyDocumentationContext
{
    public XmlDocument Document { get; internal set; }

    public bool HasXmlDocumentation => Document is not null;
}
