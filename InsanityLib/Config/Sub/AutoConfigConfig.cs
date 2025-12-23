using System.ComponentModel;

namespace InsanityLib.Config.Auto;

public class AutoConfigConfig
{
    /// <summary>
    /// Wether documentation should be put inside the config files, so that users know what the values do.
    /// </summary>
    [DefaultValue(true)]
    public bool DocumentationInConfigFile { get; set; } = true;

    /// <summary>
    /// If enabled the config will be saved to disk again right after loading.
    /// This helps ensure that newly added properties, auto corrections, documentation, etc. are saved to disk.
    /// </summary>
    [DefaultValue(true)]
    public bool SaveOnLoad { get; set; } = true;

    /// <summary>
    /// Wether elements should automatically be ordered for better visibility.
    /// (this for instances moves CollapseHeaders down to the bottom of the section)
    /// </summary>
    public bool AutoElementOrdering { get; set; } = true;
}
