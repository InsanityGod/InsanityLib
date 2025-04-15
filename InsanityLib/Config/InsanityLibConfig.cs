using InsanityLib.Attributes.Auto.Config;
using InsanityLib.Enums.Auto.Commands;
using System.ComponentModel;

namespace InsanityLib.Config
{
    public class InsanityLibConfig
    {
        [AutoConfig("InsanityLibConfig.json")] public static InsanityLibConfig Instance { get; set; }


        [DefaultValue("InsanityLib/Logs/Debug.log")]
        public string DebugLogPath { get; set; } = "InsanityLib/Logs/Debug.log";

        [DefaultValue("Labeled Chest")]
        public string DefaultLabelName { get; set; } = "labeled Chest"; //TODO simplify so even this `= "labeled chest"` is no longer needed

        [DefaultValue("Lemons")]
        public string speciality { get; set; } = null;

        //TODO remove test values
        public bool EnableAutoUI { get; set; } = true;

        /// <summary>
        /// The default parameter provider
        /// </summary>
        [DefaultValue(EParamProvider.ServiceProvider)]
        public EParamProvider DefaultParamProvider { get; set; } = EParamProvider.ServiceProvider;
    }
}
