using InsanityLib.Generators.Attributes;
using InsanityLib.Util.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Xml;

namespace InsanityLib.Documentation;

public class AssemblyDocumentationContext(Assembly assembly) : IInitialize
{
    public readonly Assembly Assembly = assembly;

    [AutoClear]
    internal static readonly Dictionary<Assembly, AssemblyDocumentationContext> Cache = [];

    public static void ClearCache() => Cache.Clear();

    public static AssemblyDocumentationContext GetForAssembly(Assembly assembly)
    {
        if(Cache.TryGetValue(assembly, out var context)) return context;

        context = new(assembly);
        context.Initialize(InsanityLibModSystem.GlobalServiceContainer);

        return context;
    }

    public void Initialize(IServiceProvider serviceProvider)
    {
        if (Assembly is null || Assembly.IsDynamic) return;

        var xmlPath = Path.Combine(
            Path.GetDirectoryName(Assembly.Location)!,
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

    public XmlDocument? Document { get; internal set; }

    [MemberNotNullWhen(true, nameof(Document))]
    public bool HasXmlDocumentation => Document is not null;
}
