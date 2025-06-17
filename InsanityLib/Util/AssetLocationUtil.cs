using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace InsanityLib.Util
{
    public static class AssetLocationUtil
    {
        public static AssetLocation FillWildCard(this AssetLocation location, ReadOnlySpan<char> filler) => new(location.Domain, location.Path.Replace("*", filler.ToString()));
    }
}
