using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Constants
{
    public static class Logging
    {
        public const string ExecutionFailedTemplate = "[InsanityLib] failed executing {0} on {1}: {2}";
        
        public const string ExecutionFailedDefaultTemplate = "[InsanityLib] failed executing {0} on {1}, using default value: {2}";
    }
}
