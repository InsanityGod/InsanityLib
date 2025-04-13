using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace InsanityLib.Contexts.Documentation
{
    public class MemberDocumentationContext
    {
        public AssemblyDocumentationContext AssemblyDocumentationContext { get; init; }
        public MemberInfo Member { get; init; }
        public XmlNode MemberNode { get; internal set; }
        public bool HasXmlDocumentation => MemberNode != null;

        public string GetDescription()
        {
            if (HasXmlDocumentation)
            {
                var summaryNode = MemberNode.SelectSingleNode("summary");
                if (summaryNode != null)
                {
                    var summaryStr = summaryNode.InnerText.Trim(Naming.TrimCharacters);
                    if(!string.IsNullOrEmpty(summaryStr)) return summaryStr;
                }
            }

            var attr = Member.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description.Trim(Naming.TrimCharacters) ?? string.Empty;
        }

        public string[] GetExamples()
        {
            if (HasXmlDocumentation)
            {
                var exampleNodes = MemberNode.SelectNodes("example");
                if (exampleNodes != null)
                {
                    var exampleStrings = new List<string>();

                    foreach (XmlNode exampleNode in exampleNodes)
                    {
                        var exampleStr = exampleNode.InnerText.Trim(Naming.TrimCharacters);
                        if (!string.IsNullOrEmpty(exampleStr)) exampleStrings.Add(exampleStr);
                    }

                    return exampleStrings.ToArray();
                }
            }

            return Array.Empty<string>();
        }

        public string GetReturn()
        {
            if (HasXmlDocumentation)
            {
                var returnNode = MemberNode.SelectSingleNode("returns");
                if (returnNode != null)
                {
                    var returnStr = returnNode.InnerText.Trim(Naming.TrimCharacters);
                    if(!string.IsNullOrEmpty(returnStr)) return returnStr;
                }
            }
            return string.Empty;
        }
    }
}
