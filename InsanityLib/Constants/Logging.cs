using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace InsanityLib.Constants
{
    public static class Logging
    {
        public const string ExecutionFailedTemplate = "[InsanityLib] failed executing {0} on {1}: {2}";
        
        public const string AutoDefaultFailed = "[InsanityLib] failed setting auto default value on static member {0}:\n{1}";
        
        public const string ExecutionFailedDefaultTemplate = "[InsanityLib] failed executing {0} on {1}, using default value instead:\n{2}";
        public const string ConfigOutdated = "Config '{0}' is outdated/invalid ('{1}' should have been '{2}' instead of '{3}')";
        public const string AutoConfigUpdate = "Config '{0}' is outdated/invalid ('{1}' should have been '{2}' instead of '{3}') attempting auto update (you should check the config afterwards)";

        public const string EncounteredValidationError = "Encountered validation error while validating '{0}', error: '{1}' at path '{2}'";
        public const string AutoFixFailed = "Could not auto fix validation error while validating '{0}', error: '{1}' at path '{2}'";
        public const string AutoFixSucceed = "Auto fixed validation error while validating '{0}', error: '{1}' at path '{2}', new value: '{3}'";

        public const string DomainDoesNotMatchFileOrigin = "Domain mismatch in '{0}', found '{1}' should have been '{2}'";
    }
}
