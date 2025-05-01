using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Util
{
    public static class NumberUtil
    {
        public static string ToPercentageString(this float percentage) => string.Format("{0:P0}", percentage);
    }
}
