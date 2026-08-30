using InsanityLib.Util.Span;

namespace InsanityLib.Util;

public static class LangUtil
{
    public static string ConcatKeyWithDomainSupport(string prefix, string path)
    {
        var code = AssetLocationSpan.Create(path, allowNoDomain: true);
        if(code.Domain.IsEmpty) return prefix + path;

        return $"{code.Domain}:{prefix}{code.Path}";
    }
}
