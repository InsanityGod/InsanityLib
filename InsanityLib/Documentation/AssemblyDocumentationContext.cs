using InsanityLib.Auto.Cleanup;
using InsanityLib.Util.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace InsanityLib.Documentation;

public class AssemblyDocumentationContext(Assembly assembly) : IInitialize
{
    public readonly Assembly Assembly = assembly;

    internal static readonly Dictionary<Assembly, AssemblyDocumentationContext> Cache = [];

    [DisposalLogic] public static void ClearCache() => Cache.Clear();

    public static AssemblyDocumentationContext GetForAssembly(Assembly assembly)
    {
        if(Cache.TryGetValue(assembly, out var context)) return context;

        context = new(assembly);
        context.Initialize();

        return context;
    }

    public void Initialize()
    {
        if (Assembly is null || Assembly.IsDynamic) return;

        var xmlPath = Path.Combine(
            Path.GetDirectoryName(Assembly.Location),
            Path.GetFileNameWithoutExtension(Assembly.Location) + ".xml"
        );

        try
        {
            if (File.Exists(xmlPath))
            {
                var doc = new XmlDocument();
                doc.Load(xmlPath);
                Document = doc;
            }
        }
        catch
        {
            //Ignore for now
        }
    }

    public XmlDocument Document { get; internal set; }

    public bool HasXmlDocumentation => Document is not null;
}
