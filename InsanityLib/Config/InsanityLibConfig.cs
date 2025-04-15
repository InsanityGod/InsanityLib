using InsanityLib.Attributes.Auto.Config;
using InsanityLib.Enums.Auto.Commands;
using InsanityLib.Enums.Auto.Config;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace InsanityLib.Config
{
    public class InsanityLibConfig
    {
        [AutoConfig("InsanityLibConfig.json", ServerSync = true, CreateIfNotExist = true, DefaultOnError = true)] 
        public static InsanityLibConfig Instance { get; set; }

        //TODO: remove all these testing values when they are no longer needed
        /// <summary>
        /// Wether feature X is enabled or not.
        /// </summary>
        [DefaultValue(true)]
        public bool Enable_Feature_X { get; set; } = true;

        /// <summary>
        /// Chance that feature Y will trigger.
        /// </summary>
        [DefaultValue(0.5f)]
        [Range(0, 1)]
        public float Feature_Y_Chance { get; set; } = 0.5f;

        /// <summary>
        /// Is used to identify the mod in the logs.
        /// </summary>
        [DefaultValue("[insanitylib]")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "The Debug_Identifier has to be configured, otherwise it will crash during startup")]
        public string Debug_Identifier { get; set; } = "[insanitylib]";

        /// <summary>
        /// The version of the config file. <br />
        /// This indicateds the version of the config file and is used for upgrading the config file.
        /// </summary>
        [VersionIdentifier(1, UpgradeMode = EConfigVersionUpgradeMode.MergeIntoNew)]
        public int ConfigVersion { get; set; } = 1;
    }
}
