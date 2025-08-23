using InsanityLib.Enums.Auto.Config.UI;
using System;

namespace InsanityLib.Attributes.Auto.Config.UI;

[AttributeUsage(AttributeTargets.Property)]
public class ConfigDisplayAttribute : Attribute
{
    public EHierarchyDisplay Hierarchy { get; set; }
}
