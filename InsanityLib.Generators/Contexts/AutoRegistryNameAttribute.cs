
using InsanityLib.Generators.Enums;

namespace InsanityLib.Generators.Contexts;

public class AutoRegistryNameContext
{
    public string Schema { get; set; }

    public ENamingConvention NamingConvention { get; set; }

    public string[] RemovePrefix { get; set; }

    public string[] RemoveSuffix { get; set; }
}
