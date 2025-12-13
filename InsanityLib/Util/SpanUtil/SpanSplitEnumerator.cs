using System;

namespace InsanityLib.Util.SpanUtil;

public ref struct SpanSplitEnumerator
{
    private ReadOnlySpan<char> _span;
    private readonly char _separator;

    public SpanSplitEnumerator(ReadOnlySpan<char> span, char separator)
    {
        _span = span;
        _separator = separator;
        Current = default;
    }

    public ReadOnlySpan<char> Current { get; private set; }

    public readonly SpanSplitEnumerator GetEnumerator() => this;

    public bool MoveNext()
    {
        if (_span.IsEmpty) return false;

        int index = _span.IndexOf(_separator);
        if (index == -1)
        {
            Current = _span;
            _span = [];
            return true;
        }

        Current = _span[..index];
        _span = _span[(index + 1)..];
        return true;
    }
}
