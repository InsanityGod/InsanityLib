using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace InsanityLib.Util.SpanUtil
{
    public static partial class SpanUtil
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
    }
}
