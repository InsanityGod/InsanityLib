using InsanityLib.Attributes.Auto;
using InsanityLib.Contexts.Documentation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace InsanityLib.Util
{
    public static class DocumentationUtil
    {

        private static readonly Dictionary<Assembly, AssemblyDocumentationContext> Cache = new();

        [DisposalLogic] public static void ClearCache() => Cache.Clear();

        public static AssemblyDocumentationContext GetDocumentationContext(this Assembly assembly)
        {
            if (Cache.TryGetValue(assembly, out var context)) return context;
            
            //TODO check if embedded xml files where a thing
            context = new AssemblyDocumentationContext();

            if (assembly != null && !assembly.IsDynamic)
            {
                var xmlPath = Path.Combine(
                    Path.GetDirectoryName(assembly.Location),
                    Path.GetFileNameWithoutExtension(assembly.Location) + ".xml"
                );

                try
                {
                    if (File.Exists(xmlPath))
                    {
                        var doc = new XmlDocument();
                        doc.Load(xmlPath);
                        context.Document = doc;
                    }
                }
                catch
                {
                    //TODO maybe log this?
                }
            }

            Cache.Add(assembly, context);
            return context;
        }

        //TODO figure out what to do with the Localizable attribute
        public static AssemblyDocumentationContext GetDocumentationContext(this Type type) => type == null || type.Assembly == null ? null : type.Assembly.GetDocumentationContext();
        public static MemberDocumentationContext GetDocumentationContext(this MemberInfo member)
        {
            var context = member.DeclaringType.GetDocumentationContext();

            var memberContext = new MemberDocumentationContext
            {
                AssemblyDocumentationContext = context,
                Member = member
            };

            if (context.HasXmlDocumentation)
            {
                var memberName = member.GetDocumentationMemberName();
                if(!string.IsNullOrEmpty(memberName))
                {
                    memberContext.MemberNode  = context.Document.SelectSingleNode($"/doc/members/member[@name='{memberName}']");
                }
            }

            return memberContext;
        }

        public static string GetDocumentationMemberName(this MemberInfo member) => member switch
        {
            Type type => $"T:{type.FullName}",
            MethodInfo method => $"M:{method.DeclaringType.FullName}.{method.Name}({string.Join(",", method.GetParameters().Select(p => p.ParameterType.FullName))})",
            PropertyInfo property => $"P:{property.DeclaringType.FullName}.{property.Name}",
            FieldInfo field => $"F:{field.DeclaringType.FullName}.{field.Name}",
            EventInfo eventInfo => $"E:{eventInfo.DeclaringType.FullName}.{eventInfo.Name}",
            _ => null,
        };
    }
}

