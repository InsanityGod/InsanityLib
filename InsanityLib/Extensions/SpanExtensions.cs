using InsanityLib.Util.Span;
using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace InsanityLib.Extensions;

public static partial class SpanExtensions
{
    public static SpanSplitEnumerator Split(this ReadOnlySpan<char> span, char separator) => new(span, separator);

    public static ReadOnlySpan<char> FirstCodePartAsSpan(this RegistryObject obj, int posFromLeft = 0) => FirstCodePartAsSpan(obj.Code?.Path, posFromLeft);
    public static ReadOnlySpan<char> FirstCodePartAsSpan(this ReadOnlySpan<char> pathSpan, int posFromLeft = 0)
    {
        if (pathSpan.IsEmpty) return pathSpan;
    
        if (posFromLeft == 0 && !pathSpan.Contains('-')) return pathSpan;
    
        int start = 0;
        int partIndex = 0;
    
        for (int i = 0; i <= pathSpan.Length; i++)
        {
            if (i == pathSpan.Length || pathSpan[i] == '-')
            {
                if (partIndex == posFromLeft) return pathSpan[start..i];
    
                partIndex++;
                start = i + 1;
            }
        }

        return default;
    }

    public static ReadOnlySpan<char> WithoutSuffix(this ReadOnlySpan<char> span, ReadOnlySpan<char> suffix) => suffix.IsEmpty || span.Length < suffix.Length || !span.EndsWith(suffix) ? span : span[..^suffix.Length];
    public static ReadOnlySpan<char> WithoutPrefix(this ReadOnlySpan<char> span, ReadOnlySpan<char> prefix) => prefix.IsEmpty || span.Length < prefix.Length || !span.StartsWith(prefix) ? span : span[prefix.Length..];

    //TODO NthSegmentRangeMethod
    public static int NthIndexOf(this ReadOnlySpan<char> span, char target, int n)
    {
        int count = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == target)
            {
                if (count == n) return i;
                count++;
            }
        }
        return -1; // Not found
    }

    public static ReadOnlySpan<char> Until(this ReadOnlySpan<char> span, ReadOnlySpan<char> target, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        var index = span.IndexOf(target, stringComparison);
        return index == -1 ? span : span[..index];
    }
    public static ReadOnlySpan<char> UntilLast(this ReadOnlySpan<char> span, ReadOnlySpan<char> target, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        var index = span.LastIndexOf(target, stringComparison);
        return index == -1 ? span : span[..index];
    }

    public static ReadOnlySpan<char> From(this ReadOnlySpan<char> span, ReadOnlySpan<char> target, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        var index = span.IndexOf(target, stringComparison);
        return index == -1 ? span : span[(index + target.Length)..];
    }

    public static ReadOnlySpan<char> FromLast(this ReadOnlySpan<char> span, ReadOnlySpan<char> target, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        var index = span.LastIndexOf(target, stringComparison);
        return index == -1 ? span : span[(index + target.Length)..];
    }
}
