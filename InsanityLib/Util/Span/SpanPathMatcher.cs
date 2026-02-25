using System;
using System.Text.RegularExpressions;

namespace InsanityLib.Util.Span;

public readonly ref struct SpanPathMatcher
{
    private readonly ReadOnlySpan<char> _prefix;
    private readonly ReadOnlySpan<char> _suffix;
    private readonly ReadOnlySpan<char> _needle;

    private readonly Regex? _regex;

    public SpanPathMatcher(ReadOnlySpan<char> needle, bool allowCompile = false)
    {
        if (needle.IsEmpty) throw new ArgumentException("Needle cannot be empty", nameof(needle));

        var forcedRegex = needle[0] == '@';
        if (forcedRegex || needle.Count('*') > 2)
        {
            var regexOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
            if(allowCompile) regexOptions |= RegexOptions.Compiled;
            _regex = new Regex(string.Create(needle.Length + (forcedRegex ? 1 : 2), needle, (span, n) =>
            {
                //@needle -> ^needle$
                span[0] = '^';
                n[(span.Length == n.Length + 1 ? 1 : 0)..].CopyTo(span[1..n.Length]);
                span[^1] = '$';
            }), regexOptions);
            return;
        }

        var wildcardIndex = needle.IndexOf('*');
        if (wildcardIndex != -1)
        {
            _prefix = needle[..wildcardIndex];
            needle = needle[(wildcardIndex + 1)..];

            wildcardIndex = needle.IndexOf('*');
            if (wildcardIndex != -1)
            {
                _suffix = needle[(wildcardIndex + 1)..];
                needle = needle[..wildcardIndex];
            }
            else
            {
                _suffix = needle;
                needle = default;
            }
        }

        _needle = needle;
    }

    public bool IsMatch(ReadOnlySpan<char> haystack)
    {
        if (_regex is not null) return _regex.IsMatch(haystack);

        if (_prefix.IsEmpty && _suffix.IsEmpty) return haystack.Equals(_needle, StringComparison.OrdinalIgnoreCase);

        return MatchFast(haystack);
    }

    private bool MatchFast(ReadOnlySpan<char> haystack)
    {
        if(haystack.Length < _prefix.Length + _suffix.Length + _needle.Length) return false;

        if(!_prefix.IsEmpty && !haystack.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase)) return false;
        if(!_suffix.IsEmpty && !haystack.EndsWith(_suffix, StringComparison.OrdinalIgnoreCase)) return false;
        if(!_needle.IsEmpty && !haystack[_prefix.Length..^_suffix.Length].Contains(_needle, StringComparison.OrdinalIgnoreCase)) return false;

        return true;
    }
}