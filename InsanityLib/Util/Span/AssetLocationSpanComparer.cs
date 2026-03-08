using System.Collections.Generic;
using Vintagestory.API.Common;

namespace InsanityLib.Util.Span;

public class AssetLocationSpanComparer : IEqualityComparer<AssetLocation>, IAlternateEqualityComparer<AssetLocationSpan, AssetLocation>
{
    public static readonly AssetLocationSpanComparer Instance = new();
    
    protected AssetLocationSpanComparer() { }

    public bool Equals(AssetLocation? x, AssetLocation? y) => x == y;
    
    public int GetHashCode(AssetLocation obj) => obj.GetHashCode();

    public bool Equals(AssetLocationSpan alternate, AssetLocation other) => alternate == other;

    public int GetHashCode(AssetLocationSpan alternate) => alternate.GetHashCode();

    public AssetLocation Create(AssetLocationSpan alternate) => alternate.Materialize();
}
