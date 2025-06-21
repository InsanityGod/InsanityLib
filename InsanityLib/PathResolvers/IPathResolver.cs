using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace InsanityLib.PathResolvers
{
    public interface IPathResolver
    {
        public string Scheme { get; }
        public bool TryResolvePath(ReadOnlySpan<char> path, ICoreAPI api, out object result);
    }
}
