using InsanityLib.Attributes.Auto.Config;

namespace InsanityLib.Config;

public class InsanityLibConfig
{
    [AutoConfig("InsanityLibConfig.json", ServerSync = false, CreateIfNotExist = true, DefaultOnError = true)] 
    public static InsanityLibConfig Instance { get; set; }
    
    public AutoConfigConfig AutoConfig { get; set; } = new AutoConfigConfig();

}
