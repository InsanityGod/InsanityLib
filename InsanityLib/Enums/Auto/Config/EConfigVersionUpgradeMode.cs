using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Enums.Auto.Config
{
    public enum EConfigVersionUpgradeMode
    {
        /// <summary>
        /// Merge the current value into a new instance of the config object
        /// </summary>
        MergeIntoNew,
        /// <summary>
        /// Throw an exception if the version is not valid
        /// </summary>
        Throw,
        /// <summary>
        /// Log a warning if the version is not valid
        /// </summary>
        Warning
    }
}
