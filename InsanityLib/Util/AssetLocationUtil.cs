using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace InsanityLib.Util
{
    public static class AssetLocationUtil
    {
        public static AssetLocation FillWildCard(this AssetLocation location, ReadOnlySpan<char> filler) => new(location.Domain, location.Path.Replace("*", filler.ToString()));

        public static string ToStringSimple(this AssetLocation location)
        {
            if(string.IsNullOrEmpty(location.Domain)) return location.Path;
            return $"{location.Domain}:{location.Path}";
        }
    }
}
