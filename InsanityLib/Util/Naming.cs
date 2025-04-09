using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace InsanityLib.Util
{
    public static class Naming
    {
        internal static readonly char[] ReadableSplitIdentifiers = new char[] { '-', '_', ':' };

        public static string ToHumanReadable(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return string.Empty;

            StringBuilder newText = new(str.Length * 2);
            newText.Append(str[0]);

            for (int i = 1; i < str.Length; i++)
            {
                if (char.IsUpper(str[i]) && str[i - 1] != ' ')
                {
                    newText.Append(' ');
                }

                newText.Append(str[i]);
            }

            foreach (var delimiter in ReadableSplitIdentifiers)
            {
                newText.Replace(delimiter, ' ');
            }

            return newText.ToString();
        }

        public static string GetHumanReadableName(this MemberInfo type)
        {
            var displayNameAttr = type.GetCustomAttribute<DisplayNameAttribute>();
            if(displayNameAttr != null) return Lang.Get(displayNameAttr.DisplayName);
            return type.Name.ToHumanReadable();
        }

        public static AssetLocation ToAssetLocation(this string str)
        {
            if(string.IsNullOrWhiteSpace(str)) return null;
            return str.Contains(':') ? (AssetLocation)str : new AssetLocation(null, str);
        }
    }
}
