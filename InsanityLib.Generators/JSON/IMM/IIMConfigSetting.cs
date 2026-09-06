using System;
using System.Collections.Generic;
using System.Text;

namespace InsanityLib.BuildTasks.IMM;

public class IIMConfigSetting
{
    public string Type { get; set; }

    /// <summary>
    /// The type of the enumeration element (for stuff like arrays)
    /// </summary>
    public string ElementType { get; set; }

    public string Label { get; set; }

    public string Description { get; set; }

    public string Map { get; set; }

    /// <summary>
    /// Not used for now, but maybe interesting for the future
    /// </summary>
    public string ConfigSide { get; set; }
}


public class IMMConfigSettingOption
{
    public string Label { get; set; }

    /// <summary>
    /// The value of the option, can be any primitive
    /// </summary>
    public object Value { get; set; }
}