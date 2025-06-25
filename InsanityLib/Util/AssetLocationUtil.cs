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

        //public static string PreparePathRegex(this AssetLocation location)
		//{
        //    var needle = location.Path;
		//	if (needle[0] == '@') return $"^{needle.AsSpan(1)}$";
        //
		//	int wildIndex = needle.IndexOf('*');
		//	if (wildIndex == -1) return null;
        //
		//	if (needle[0] != '^' && needle.IndexOf('*', wildIndex + 1) < 0) return needle[(wildIndex + 1)..];
		//	
        //    needle = Regex.Escape(needle).Replace("\\*", ".*");
		//	return $"^{needle}$";
		//}
    }
}
