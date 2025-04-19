using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Enums.Auto.Commands
{
    public enum EParamSource
    {
        /// <summary>
        /// Provided from the one calling it
        /// </summary>
        Caller = 0,
        
        /// <summary>
        /// Provided from what the one calling it is targeting (looking at)
        /// </summary>
        CallerTarget = 1,

        /// <summary>
        /// Provided from the command itself
        /// </summary>
        Specify = 2,
    }
}
