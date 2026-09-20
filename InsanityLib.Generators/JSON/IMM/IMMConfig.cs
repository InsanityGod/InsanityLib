using System;
using System.Collections.Generic;
using System.Text;

namespace InsanityLib.BuildTasks.IMM;

public class IMMConfigColection
{
    public List<IMMConfig> Configuration { get; set; } = [];

    //TODO Dependencies
}


public class IMMConfig
{
    public string ConfigFile { get; set; }

    public string ConfigLabel { get; set; }

    public string Description { get; set; }

    public string ConfigSource { get; set; } = "ModConfig";

    public string ConfigSide { get; set; }

    public List<IIMConfigSetting> Settings { get; set; } = [];
}
