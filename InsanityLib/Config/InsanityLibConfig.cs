using InsanityLib.Attributes.Auto;
using InsanityLib.Enums.Auto.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Config
{
    public class InsanityLibConfig
    {
        //TODO AutoConfig attribute
        [AutoConfig("InsanityLibConfig.json")]
        public static InsanityLibConfig Instance { get; set; }

        [DefaultValue("InsanityLib/Logs/Debug.log")]
        public string DebugLogPath { get; set; } = "InsanityLib/Logs/Debug.log";

        [DefaultValue("Labeled Chest")]
        public string DefaultLabelName { get; set; } = "labeled Chest";

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
