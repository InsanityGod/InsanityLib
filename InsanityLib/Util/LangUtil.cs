using InsanityLib.Util.Span;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace InsanityLib.Util;

public static partial class LangUtil
{
    public static string ConcatKeyWithDomainSupport(string prefix, string path)
    {
        var code = AssetLocationSpan.Create(path, allowNoDomain: true);
        if(code.Domain.IsEmpty) return prefix + path;

        return $"{code.Domain}:{prefix}{code.Path}";
    }

    [GeneratedRegex(@"\{(\d+)(?=[,:}])")]
    private static partial Regex PlaceholderRegex();

    //TODO see if I can find a better solution to merge format strings
    public static string CombineUnformatted(this IEnumerable<string> formats,string separator)
    {
        var offset = 0;

        return string.Join(separator, formats.Select(format =>
        {
            var start = offset;
            var maxIndex = -1;

            var result = PlaceholderRegex().Replace(format, match =>
            {
                var index = int.Parse(match.Groups[1].Value);

                maxIndex = Math.Max(maxIndex, index);

                return $"{{{index + start}";
            });

            offset += maxIndex + 1;

            return result;
        }));
    }
}
