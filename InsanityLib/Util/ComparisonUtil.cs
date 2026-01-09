using InsanityLib.Extensions;
using System;
using System.Linq;
using Vintagestory.API.Common;

namespace InsanityLib.Util;

public static class ComparisonUtil
{
    //TODO CompareWithoutVariantMethod
    public static bool CompareWithoutOrientation(RegistryObject item1, RegistryObject item2)
    {
        if (item1.Code is null || item2.Code is null || item1.Code.Domain != item2.Code.Domain) return false;

        var orientationVariantIndex = item1.GetOrientationVariantIndex();
        if (orientationVariantIndex != item2.GetOrientationVariantIndex()) return false;
        ReadOnlySpan<char> path1 = item1.Code.Path;
        ReadOnlySpan<char> path2 = item2.Code.Path;
        
        if(orientationVariantIndex == -1) return path1.SequenceEqual(path2);
        
        //Find nth occurence of '-'
        var index1 = path1.NthIndexOf('-', orientationVariantIndex);
        var index2 = path2.NthIndexOf('-', orientationVariantIndex);
        if (!path1[..index1].SequenceEqual(path2[..index2])) return false;
        
        index1 = path1.NthIndexOf('-', orientationVariantIndex + 1);
        index2 = path2.NthIndexOf('-', orientationVariantIndex + 1);

        if(index1 == -1) index1 = path1.Length;
        if(index2 == -1) index2 = path1.Length;
        if (!path1[index1..].SequenceEqual(path2[index2..])) return false;

        return true;
    }
}
