using InsanityLib.Attributes.Auto.Config;
using InsanityLib.Enums.Auto.Commands;
using InsanityLib.Enums.Auto.Config;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Vintagestory.API.Common;

namespace InsanityLib.Config
{
    public class InsanityLibConfig
    {
        [AutoConfig("InsanityLibConfig.json", ServerSync = true, CreateIfNotExist = true, DefaultOnError = true)] 
        public static InsanityLibConfig Instance { get; set; }

        /// <summary>Wether documentation should be put inside the config files, so that users know what the values do.</summary>
        [DefaultValue(true)]
        public bool DocumentConfigFiles { get; set; } = true;

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
        

        /// <summary>
        /// Just some number
        /// </summary>
        public float SomeNumber { get; set; } = 0.5f;

        /// <summary>
        /// Test value
        /// </summary>
        [DefaultValue(EParamSource.Caller)]
        public EParamSource Source { get; set; } = EParamSource.Caller;
        
        public TestEnum Test { get; set; } = TestEnum.SomeValue | TestEnum.RandomValue;
        
        public EnumAppSide TestComplexEnumFlag { get; set; } = EnumAppSide.Universal;

        public AutoUpdateConfig UpdateConfiguration { get; set; } = new AutoUpdateConfig();

        [Flags]
        public enum TestEnum
        {
            SomeValue = 1,
            SomeOtherValue = 2,
            RandomValue = 4,
        }
    }
}
