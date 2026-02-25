using InsanityLib.Constants;
using InsanityLib.Extensions;
using InsanityLib.Util.Span;
using System;
using System.Linq;
using Vintagestory.API.Common;

namespace InsanityLib.PathResolvers.Implementations;

public class WorldPropertiesResolver : IPathResolver
{
    public string Scheme => "worldproperties";

    public bool TryResolvePath(ReadOnlySpan<char> path, ICoreAPI api, out object? result)
    {
        var assets = api.Assets.GetAssets(AssetCategory.worldproperties);
        var assetLocation = AssetLocationSpan.Create(path, allowNoDomain: true);

        foreach (var asset in assets)
        {
            //Magic number 16 is the length of "worldproperties/" and 5 is the length of ".json"
            if(!assetLocation.DomainSatifies(asset.Location.Path) || !path.SequenceEqual(asset.Location.Path.AsSpan()[16..^5])) continue;

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
