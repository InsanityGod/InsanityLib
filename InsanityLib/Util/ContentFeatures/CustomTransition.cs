using InsanityLib.Extended.Transitions;
using InsanityLib.Extensions;
using InsanityLib.Generators.Attributes;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace InsanityLib.Util.ContentFeatures;

public static class CustomTransition
{
    [AutoClear]
    internal readonly static Dictionary<AssetLocation, Type> ClassRegistry = [];
    
    public static void RegisterHandler<T>(AssetLocation code) where T : ITransitionHandler => RegisterHandler(code, typeof(T));
    
    internal static void RegisterHandler(AssetLocation code, Type type) => ClassRegistry[code] = type;

    public static ExtendedTransition ExtendedEnum => (ExtendedTransition)Extended.Enums.ExtendedEnum.EnumExtensions[typeof(EnumTransitionType)];

    //TODO Auto generate this!
    public static void LoadAssets(ICoreAPI api)
    {
        var serviceProvider = api.GetServiceProvider();
        var logger = serviceProvider.GetService<ILogger>();
        
        foreach ((var origin, var transitionType) in api.Assets.GetMany<TransitionType>(logger, "transitiontypes/"))
        {
            transitionType.Code.EnsureCorrectDomainForAsset(origin, logger);

            ExtendedEnum.RegisterTransitionType(serviceProvider, logger, transitionType);
        }
    }
}
