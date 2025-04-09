using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Attributes.Auto.Harmony
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AutoPatcherAttribute : Attribute
    {
        public readonly string HarmonyId;

        public AutoPatcherAttribute(string harmonyId)
        {
            if (string.IsNullOrEmpty(harmonyId)) throw new ArgumentException($"'{nameof(harmonyId)}' cannot be null or empty.", nameof(harmonyId));
            HarmonyId = harmonyId;
        }
    }
}
