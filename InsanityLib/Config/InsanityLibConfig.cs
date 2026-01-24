using InsanityLib.Auto.Config;
using InsanityLib.Config.Sub;
using InsanityLib.Generators.Attributes;
using Vintagestory.API.Common;

namespace InsanityLib.Config;

public class InsanityLibConfig
{
    [AutoConfig("InsanityLibConfig.json", ServerSync = false)] 
    public static InsanityLibConfig? Instance { get; set; }
    
    public AutoConfigConfig AutoConfig { get; set; } = new AutoConfigConfig();

}
