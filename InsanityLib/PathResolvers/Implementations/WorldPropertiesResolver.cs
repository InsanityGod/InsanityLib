using HarmonyLib;
using InsanityLib.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;

namespace InsanityLib.PathResolvers.Implementations;

public class WorldPropertiesResolver : IPathResolver
{
    public string Scheme => "worldproperties";

    public bool TryResolvePath(ReadOnlySpan<char> path, ICoreAPI api, out object result)
    {
        var assets = Traverse.Create(api.Assets).Field<IDictionary<string, List<IAsset>>>("assetsByCategory").Value[AssetCategory.worldproperties.Code];
        var domainSeperatorIndex = path.IndexOf(':');
        ReadOnlySpan<char> domain;
        if(domainSeperatorIndex == -1) domain = default;
        else
        {
            domain = path[..domainSeperatorIndex];
            path = path[(domainSeperatorIndex + 1)..]; //Skip the domain separator
        }

        foreach (var asset in assets)
        {
            //Magic number 16 is the length of "worldproperties/" and 5 is the length of ".json"
            if((!domain.IsEmpty && !domain.SequenceEqual(asset.Location.Domain)) || !path.SequenceEqual(((ReadOnlySpan<char>)asset.Location.Path)[16..^5])) continue;

            if (!asset.IsLoaded()) asset.Origin.LoadAsset(asset);

            var property = asset.ToObject<StandardWorldProperty>();

            result = property.Variants.Select(static variant => variant.Code.Path);
            return true;
        }

        api.Logger.Warning(
            Logging.PathResolverFailed,
            nameof(AutoConfigResolver),
            string.Empty,
            path.ToString(),
            "Worldproperty not found"
        );

        result = null;
        return false;
    }
}
