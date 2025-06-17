using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace InsanityLib.Util
{
    public static class ComparisonUtil
    {
        public static bool CompareWithoutOrientation(RegistryObject item1, RegistryObject item2)
        {
            if (item1.Code == null || item2.Code == null || item1.Code.Domain != item2.Code.Domain) return false;

            var orientationVariantIndex = item1.GetOrientationVariantIndex();
            if (orientationVariantIndex != item2.GetOrientationVariantIndex()) return false;
            if(orientationVariantIndex != -1) orientationVariantIndex++; //First segment is not considered a variant (but only if the variant was found)
            
            ReadOnlySpan<char> path1 = item1.Code.Path.AsSpan();
            ReadOnlySpan<char> path2 = item2.Code.Path.AsSpan();

            int segIdx = 0;
            int start1 = 0, start2 = 0;
            int skipLength1 = 0;
            int skipLength2 = 0;
            while (start1 < path1.Length && start2 < path2.Length)
            {
                int end1 = path1[start1..].IndexOf('-');
                end1 = end1 == -1 ? path1.Length : start1 + end1;
                
                int end2 = path2[start2..].IndexOf('-');
                end2 = end2 == -1 ? path2.Length : start2 + end2;

                if (segIdx != orientationVariantIndex)
                {
                    var seg1 = path1[start1..end1];
                    var seg2 = path2[start2..end2];
                    if (!seg1.SequenceEqual(seg2)) return false;
                }
                else
                {
                    skipLength1 = end1 - start1;
                    skipLength2 = end2 - start2;
                }

                //if (end1 == path1.Length && end2 == path2.Length) break;

                start1 = end1 + 1;
                start2 = end2 + 1;
                segIdx++;
            }

            // Ensure both paths have the same number of segments
            if ((path1.Length - skipLength1) != (path2.Length - skipLength2)) return false;

            return true;
        }
    }
}
