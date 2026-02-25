using System;
using System.Text.RegularExpressions;
using Vintagestory.API.Common;

namespace InsanityLib.Util.Span;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
public readonly ref struct AssetLocationSpan : IEquatable<AssetLocationSpan>
{
    public readonly ReadOnlySpan<char> Domain;

    public readonly ReadOnlySpan<char> Path;

    public AssetLocationSpan(AssetLocation assetLocation) : this(assetLocation.Domain, assetLocation.Path) { }

    public AssetLocationSpan(ReadOnlySpan<char> domain, ReadOnlySpan<char> path)
    {
        Domain = domain;
        Path = path;
    }

    public static AssetLocationSpan Create(ReadOnlySpan<char> domainAndPath, ReadOnlySpan<char> defaultDomain = default, bool allowNoDomain = false)
    {
        var index = domainAndPath.IndexOf(':');

        if (index == -1)
        {
            return new AssetLocationSpan((!allowNoDomain && defaultDomain.IsEmpty) ? "game" : defaultDomain, domainAndPath);
        }
        else return new AssetLocationSpan(domainAndPath[..index], domainAndPath[(index + 1)..]);
    }

    public static implicit operator AssetLocationSpan(AssetLocation assetLocation) => new(assetLocation);

    public static implicit operator AssetLocationSpan(string domainAndPath) => Create(domainAndPath);

    public bool Equals(AssetLocationSpan other) => Domain.SequenceEqual(other.Domain) && Path.SequenceEqual(other.Path);

    public static bool operator ==(AssetLocationSpan left, AssetLocationSpan right) => left.Equals(right);

    public static bool operator !=(AssetLocationSpan left, AssetLocationSpan right) => !left.Equals(right);

    public override int GetHashCode() => string.GetHashCode(Domain) ^ string.GetHashCode(Path);

    /// <summary>
    /// Creates a new <see cref="AssetLocation"/> from this span.<br/>
    /// Note that this involves allocating new strings, so it should be avoided if possible.
    /// </summary>
    public AssetLocation Materialize() => new(Domain.IsEmpty ? "game" : Domain.ToString(), Path.ToString());

    /// <summary>
    /// Checks wether the other domain satisfies this domain.<br/>
    /// This supports leaving the domain empty or using `*` to make this always return true.
    /// </summary>
    public bool DomainSatifies(ReadOnlySpan<char> otherDomain)
    {
        if(Domain.IsEmpty || (Domain.Length == 1 && Domain[0] == '*')) return true;

        return Domain.SequenceEqual(otherDomain);
    }

    /// <summary>
    /// Get a wildcard matcher for fast comparison of the path.<br/>
    /// </summary>
    public SpanPathMatcher GetPathMatcher() => new(Path);
}
