using InsanityLib.Generators.Attributes.Enums;
using System.Diagnostics;

namespace InsanityLib.Generators.Attributes;

[Conditional("CompileTimeOnly")]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class)]
public class AutoRegistryNameAttribute(string scheme = "{modid}:{name}", ENamingConvention namingConvention = ENamingConvention.PascalCase, string[] removePrefix = null, string[] removeSuffix = null) : Attribute
{
    public string Scheme { get; } = scheme;

    public ENamingConvention NamingConvention { get; } = namingConvention;

    public string[] RemovePrefix { get; set; } = removePrefix;

    public string[] RemoveSuffix { get; set; } = removeSuffix;
}
