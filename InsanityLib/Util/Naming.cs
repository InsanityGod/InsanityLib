using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
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
        public static readonly char[] TrimCharacters = new char[] { ' ', '\n', '\r', '\t' };
        public static readonly char[] ReadableSplitIdentifiers = new char[] { '-', '_', ':' };

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

        public static string GetHumanReadableName(this MemberInfo member)
        {
            var displayNameAttr = member.GetCustomAttribute<DisplayNameAttribute>();
            if(displayNameAttr != null) return Lang.Get(displayNameAttr.DisplayName);
            return member.Name.ToHumanReadable();
        }

        public static string GetRegistryName(this MemberInfo member, string domain = null)
        {
            //TODO allow for removing prefixes
            var memberName = member.Name.ToLower();

            if(!string.IsNullOrEmpty(domain)) return $"{domain}:{memberName}";
            return memberName;
        }

        /// <summary>
        /// Converts a string to an AssetLocation in a way that does not automatically add the default domain <br/>
        /// Meaning that if you parse it back it will be the same as the input string
        /// </summary>
        public static AssetLocation ToAssetLocation(this string str)
        {
            if(string.IsNullOrWhiteSpace(str)) return null;
            return str.Contains(':') ? (AssetLocation)str : new AssetLocation(null, str);
        }

        public static string EnsureFileExtension(this string str, string extension)
        {
            if (string.IsNullOrWhiteSpace(str) || string.IsNullOrWhiteSpace(extension)) return str;
            if (!extension.StartsWith('.')) extension = '.' + extension;

            string currentExtension = Path.GetExtension(str);
            if (string.IsNullOrEmpty(currentExtension)) return str + extension;
            return string.Concat(str.AsSpan(0, str.Length - currentExtension.Length), extension);
        }
    }
}
