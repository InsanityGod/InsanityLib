using InsanityLib.Constants;
using InsanityLib.Handlers;
using InsanityLib.JsonAssets;
using InsanityLib.Util;
using InsanityLib.Util.ContentFeatures;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace InsanityLib.Extended;

public sealed class ExtendedTransition : ExtendedEnum
{
    public ExtendedTransition() : base(typeof(EnumTransitionType)) { }

    private readonly Dictionary<EnumTransitionType, TransitionHandler> HandlerLookup = new();

    public void RegisterTransitionType(IServiceProvider provider, TransitionType transitionType)
    {
        if(OffsetLookup.ContainsKey(transitionType.Code.ToString())) return;
        
        if(!CustomTransition.ClassRegistry.TryGetValue(transitionType.Handler, out var transitionHandlerType))
        {
            provider.GetService<ILogger>()?.Error(Logging.ExecutionFailedTemplate, nameof(RegisterTransitionType), transitionType.Code, $"No such transitionHandler '{transitionType.Handler}'");
            return;
        }


        if (transitionHandlerType.AutoCreate(provider, true) is not TransitionHandler handler)
        {
            provider.GetService<ILogger>()?.Error(Logging.ExecutionFailedTemplate, nameof(RegisterTransitionType), transitionType.Code, $"Could not instantiate '{transitionHandlerType.FullName}'");
            return;
        }

        try
        {
            handler.TransitionType = (EnumTransitionType)currentOffset;
            handler.TransitionCode = transitionType.Code;
            handler.LoadAttributes(transitionType.Attributes);
        }
        catch(Exception ex)
        {
            provider.GetService<ILogger>()?.Error(Logging.ExecutionFailedTemplate, nameof(TransitionHandler.LoadAttributes), transitionHandlerType.FullName, ex);
            return;
        }
        HandlerLookup[(EnumTransitionType)currentOffset] = handler;
        OffsetLookup[transitionType.Code.ToString()] = currentOffset++;
    }

    public TransitionHandler FindHandler(EnumTransitionType value) => HandlerLookup.GetValueOrDefault(value);
}
