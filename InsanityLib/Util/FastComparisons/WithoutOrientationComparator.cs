using InsanityLib.Extensions;
using System;
using System.Linq;
using Vintagestory.API.Common;

namespace InsanityLib.Util.FastComparisons;

public readonly ref struct WithoutOrientationComparator
{
    public readonly string Domain;

    public readonly int OrientationIndex;

    public readonly ReadOnlySpan<char> StartComparison;
    
    public readonly ReadOnlySpan<char> EndComparison;

    public WithoutOrientationComparator(CollectibleObject compareTarget)
    {
        Domain = compareTarget.Code.Domain; //Result will always be false if this is null so user should check for that themself
        OrientationIndex = compareTarget.GetOrientationVariantIndex();
        
        ReadOnlySpan<char> path = compareTarget.Code.Path;
        var index = path.NthIndexOf('-', OrientationIndex);
        if(index == -1)
        {
            StartComparison =  path; //No orientation exists
            return;
        }
        StartComparison = path[..index];
        
        index = path.NthIndexOf('-', OrientationIndex + 1);
        if(index == -1) index = path.Length;
        EndComparison = path[index..];
    }

    public bool IsMatch(CollectibleObject item)
    {
        if(item.Code is null || Domain != item.Code.Domain || item.GetOrientationVariantIndex() != OrientationIndex) return false;
        if(OrientationIndex == -1) return StartComparison.SequenceEqual(item.Code.Path);
        
        ReadOnlySpan<char> path = item.Code.Path;
        var index = path.NthIndexOf('-', OrientationIndex);
        if(!StartComparison.SequenceEqual(path[..index])) return false;
        
        index = path.NthIndexOf('-', OrientationIndex + 1);
        if(index == -1) index = path.Length;
        if(!EndComparison.SequenceEqual(path[index..])) return false;

        return true;
    }
}
