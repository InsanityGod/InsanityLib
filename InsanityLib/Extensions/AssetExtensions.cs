using InsanityLib.PathResolvers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;
using Vintagestory.Common;

namespace InsanityLib.Extensions;

public static class AssetExtensions
{
    /// <summary>
    /// Searches for all assets in given basepath and uses Newtonsoft.Json to automatically turn them into objects.<br/>
    /// Will log an error to given ILogger if it can't parse the json file and continue with the next asset.<br/>
    /// Remember to use lower case paths.<br/>
    /// If no domain is specified, all domains will be searched.<br/>
    /// The returned list is considered unsorted.<br/>
    /// Note: this method will also apply extended asset features from InsanityLib such as: PathResolvers
    /// </summary>
    /// <typeparam name="T" />
    /// <param name="api" />
    /// <param name="logger" />
    /// <param name="pathBegins" />
    /// <param name="domain" />
    public static Dictionary<AssetLocation, T> GetManyExtended<T>(this ICoreAPI api, ILogger logger, string pathBegins, string? domain = null)
    {
        var assets = api.Assets.GetMany(pathBegins, domain, true);
        Dictionary<AssetLocation, T> results = new(assets.Count);
		foreach (IAsset asset in assets)
		{
			try
			{
                var result = asset.ToObject<JToken>();
                Resolver.ResolveAll(api, result);
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new AssetLocationJsonParser(asset.Location.Domain));
                var typedResult = result.ToObject<T>(JsonSerializer.Create(settings));
                if(typedResult is null) continue;

				results.Add(asset.Location, typedResult);
			}
			catch (Exception exception)
			{
				logger.Error("Error while loading json file '{0}' as '{1}': {2}", asset, typeof(T), exception);
			}
		}
		return results;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "assetsByCategory")]
    private static extern ref IDictionary<string, List<IAsset>> GetAssetsByCategory(AssetManager instance);

    public static IEnumerable<IAsset> GetAssets(this IAssetManager assetManager, AssetCategory assetCategory) => assetManager.GetAssets(assetCategory.Code);
    public static IEnumerable<IAsset> GetAssets(this IAssetManager assetManager, string assetCategory) => 
        GetAssetsByCategory((AssetManager)assetManager).TryGetValue(assetCategory, out var assets) ? assets : Enumerable.Empty<IAsset>();
}