using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Config
{
    public class AutoConfigConfig
    {
        /// <summary>Wether documentation should be put inside the config files, so that users know what the values do.</summary>
        [DefaultValue(true)]
        public bool DocumentationInConfigFile { get; set; } = true;

        /// <summary>
        /// If enabled the config will be saved to disk again right after loading.
        /// This helps ensure that newly added properties, auto corrections, documentation, enz. are saved to disk.
        /// </summary>
        [DefaultValue(true)]
        public bool SaveOnLoad { get; set; } = true;
    }
}
