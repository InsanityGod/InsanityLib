using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Config
{
    public class AutoUpdateConfig
    {
        /// <summary>
        /// Whether the auto update feature is enabled or not.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether the mod should check for updates on startup.
        /// </summary>
        public bool CheckForUpdates { get; set; } = true;

        /// <summary>
        /// The warning string that will be displayed when the mod is outdated.
        /// </summary>
        public string WarningString { get; set; } = "This mod is outdated! Please update to the latest version.";
    }
}
