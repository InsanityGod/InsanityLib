using System;

namespace InsanityLib.Auto.Config.ConfigLib;

[AttributeUsage(AttributeTargets.Property)]
public class ConfigDisplayAttribute : Attribute
{
    public EHierarchyDisplay Hierarchy { get; set; }
}
