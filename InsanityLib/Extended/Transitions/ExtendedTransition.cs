using InsanityLib.Constants;
using InsanityLib.Extended.Enums;
using InsanityLib.Extensions;
using InsanityLib.Util;
using InsanityLib.Util.ContentFeatures;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace InsanityLib.Extended.Transitions;

public sealed class ExtendedTransition() : ExtendedEnum(typeof(EnumTransitionType))
{
    private readonly Dictionary<EnumTransitionType, ITransitionHandler> HandlerLookup = [];

    public void RegisterTransitionType(IServiceProvider provider, ILogger logger, TransitionType transitionType)
    {
        if(OffsetLookup.ContainsKey(transitionType.Code.ToString())) return;
        
        if(!CustomTransition.ClassRegistry.TryGetValue(transitionType.Handler, out var transitionHandlerType))
        {
            logger.Error(Logging.ExecutionFailed, nameof(RegisterTransitionType), transitionType.Code, $"No such transitionHandler '{transitionType.Handler}'");
            return;
        }


        if (transitionHandlerType.AutoCreate(provider, true) is not ITransitionHandler handler)
        {
            logger.Error(Logging.ExecutionFailed, nameof(RegisterTransitionType), transitionType.Code, $"Could not instantiate '{transitionHandlerType.FullName}'");
            return;
        }

        try
        {
            handler.TransitionType = (EnumTransitionType)currentOffset;
            handler.TransitionCode = transitionType.Code;
            if(transitionType.Attributes is not null) handler.LoadAttributes(transitionType.Attributes);
        }
        catch(Exception ex)
        {
            logger.Error(Logging.ExecutionFailed, nameof(ITransitionHandler.LoadAttributes), transitionHandlerType.FullName, ex);
            return;
        }
        HandlerLookup[(EnumTransitionType)currentOffset] = handler;
        OffsetLookup[transitionType.Code.ToString()] = currentOffset++;
    }

    public ITransitionHandler? FindHandler(EnumTransitionType value) => HandlerLookup.GetValueOrDefault(value);
}
