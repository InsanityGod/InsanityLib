using InsanityLib.Documentation;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace InsanityLib.Util;

public static partial class NamingExtensions
{
    public static readonly char[] ReadableSplitIdentifiers = ['-', '_', ':'];

    [GeneratedRegex(@"[^\S\r\n]+")]
    public static partial Regex WhiteSpaceRegex();

    public static string CleanWhiteSpaces(this string input)
    {
        if(string.IsNullOrEmpty(input)) return input;

        return WhiteSpaceRegex()
            .Replace(input, " ")
            .Replace("\n ", "\n")
            .Trim();
    }

    public static string ToHumanReadable(this string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return string.Empty;

        StringBuilder newText = new(str.Length * 2);
        newText.Append(str[0]);

        for (int i = 1; i < str.Length; i++)
        {
            if (char.IsUpper(str[i]) && !char.IsUpper(str[i - 1]) && str[i - 1] != ' ')
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

    public static string GetDebugDisplayName(this MemberInfo member) => $"{member.DeclaringType?.FullName ?? "unknown"}:{member.Name}";

    private static string GetLangKey(this EDocumentationType type) => type switch
    {
        EDocumentationType.Description => "desc",
        _ => type.ToString().ToLower(),
    };

    //TODO domain support
    public static string GetLangKey(this MemberInfo member, EDocumentationType type = EDocumentationType.Name) => $"member{type.GetLangKey()}-{member.DeclaringType?.FullName}:{member.Name}".ToLower();
    
    public static string GetLangKey(this ParameterInfo parameter, EDocumentationType type = EDocumentationType.Name) => $"member{type.GetLangKey()}-{parameter.Member.DeclaringType?.FullName}:{parameter.Member.Name}.{parameter.Name}".ToLower();

    public static string GetHumanReadableName(this MemberInfo member)
    {
        var languageStringKey = member.GetLangKey(EDocumentationType.Name);
        var languageStringValue = Lang.Get(languageStringKey);
        if(languageStringValue != languageStringKey && !string.IsNullOrWhiteSpace(languageStringValue)) return languageStringValue;
        
        var displayNameAttr = member.GetCustomAttribute<DisplayNameAttribute>();
        if(displayNameAttr is not null) return Lang.Get(displayNameAttr.DisplayName);
        
        return member.Name.ToHumanReadable();
    }

    public static string GetHumanReadableName(this ParameterInfo parameter)
    {
        var languageStringKey = parameter.GetLangKey(EDocumentationType.Name);
        var languageStringValue = Lang.Get(languageStringKey);
        if(languageStringValue != languageStringKey && !string.IsNullOrWhiteSpace(languageStringValue)) return languageStringValue;
        
        var displayNameAttr = parameter.GetCustomAttribute<DisplayNameAttribute>();
        if(displayNameAttr is not null) return Lang.Get(displayNameAttr.DisplayName);
        
        return parameter.Name.ToHumanReadable();
    }

    public static readonly string[] RegistryAffixes =
    [
        "Item",
        "Block",
        "BlockEntity",
        "Entity",
        "Behavior",
        "CollectibleBehavior",
        "BlockBehavior",
        "BlockEntityBehavior",
        "TransitionHandler"
    ];

    public static string GetRegistryName(this MemberInfo member, string domain = null, bool removeComminAffixes = false)
    {
        //TODO attributes
        var memberName = member.Name;

        if(removeComminAffixes) foreach(var affix in RegistryAffixes) memberName = memberName.AsSpan().RemoveAffix(affix).ToString();

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

    public static bool TryRemoveFrom(this string prefix, ref string str)
    {
        if(str is null || !str.StartsWith(prefix)) return false;
        str = str[prefix.Length..];
        return true;
    }

    internal static ReadOnlySpan<char> RemoveSuffix(this ReadOnlySpan<char> str, ReadOnlySpan<char> suffix) => str.IsWhiteSpace() || suffix.IsWhiteSpace() || !str.EndsWith(suffix) ? str : str[..^suffix.Length];
    internal static ReadOnlySpan<char> RemovePrefix(this ReadOnlySpan<char> str, ReadOnlySpan<char> prefix) => str.IsWhiteSpace() || prefix.IsWhiteSpace() || !str.StartsWith(prefix) ? str : str[prefix.Length..];
    internal static ReadOnlySpan<char> RemoveAffix(this ReadOnlySpan<char> str, ReadOnlySpan<char> affix) => str.RemovePrefix(affix).RemoveSuffix(affix);

    public static string ReplaceSpecialSymbolsWithText(this string input) => input.Replace("∞", "Infinity");
}
