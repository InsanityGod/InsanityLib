using InsanityLib.BuildTasks.IMM;
using System;
using System.Collections.Generic;
using System.Text;

namespace InsanityLib.Generators;

public class ConfigTranslations
{
    public IMMConfig IMM { get; set; } = new();

}

public partial class ModSystemGenerator
{
    public Dictionary<string, ConfigTranslations> ConfigTranslations = new Dictionary<string, ConfigTranslations>();

    public void TranslateConfigs()
    {
        if(ConfigTranslations.Count > 0 || configlist.Length < 1) return; //Already translated or no configs


    }

}
