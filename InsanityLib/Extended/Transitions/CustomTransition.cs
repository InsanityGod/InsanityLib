using InsanityLib.Generators.Attributes;
using InsanityLib.Util.Span;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace InsanityLib.Extended.Transitions;

delegate ITransitionHandler CreateTransitionHandler(AssetLocation transitionCode, EnumTransitionType transitionType);
public static class CustomTransition
{

    [AutoClear]
    internal readonly static Dictionary<AssetLocation, (Type Type, CreateTransitionHandler Constructor)> ClassRegistry = new(AssetLocationSpanComparer.Instance);
    
    public static void RegisterHandler<T>(AssetLocation code) where T : ITransitionHandler, new() => ClassRegistry[code] = (typeof(T), (transitionCode, transitionType) =>
    {
        var result = new T()
        {
            TransitionCode = transitionCode,
            TransitionType = transitionType
        };
        result.Initialize(InsanityLibModSystem.GlobalServiceContainer);
        return result;
    });
   

    public static ExtendedTransition ExtendedEnum => (ExtendedTransition)Enums.ExtendedEnum.EnumExtensions[typeof(EnumTransitionType)];
}
