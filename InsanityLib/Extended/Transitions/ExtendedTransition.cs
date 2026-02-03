using InsanityLib.Constants;
using InsanityLib.Extended.Enums;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace InsanityLib.Extended.Transitions;

public sealed class ExtendedTransition() : ExtendedEnum(typeof(EnumTransitionType))
{
    private readonly Dictionary<EnumTransitionType, ITransitionHandler> HandlerLookup = [];

    public void RegisterTransitionType(ILogger logger, TransitionType transitionType)
    {
        if(OffsetLookup.ContainsKey(transitionType.Code.ToString())) return;
        
        if(!CustomTransition.ClassRegistry.TryGetValue(transitionType.Handler, out var registryEntry))
        {
            logger.Error(Logging.ExecutionFailed, nameof(RegisterTransitionType), transitionType.Code, $"Unknown transitionHandler '{transitionType.Handler}'");
            return;
        }

        ITransitionHandler? handler = null;
        try
        {
            handler = registryEntry.Constructor(transitionType.Code, (EnumTransitionType)currentOffset);
        }
        catch(Exception ex)
        {
            logger.Error(ex);
            return;
        }

        try
        {
            if(transitionType.Attributes is not null) handler.LoadAttributes(transitionType.Attributes);
        }
        catch(Exception ex)
        {
            logger.Error(ex);
            return;
        }

        HandlerLookup[(EnumTransitionType)currentOffset] = handler;
        OffsetLookup[transitionType.Code.ToString()] = currentOffset++;
    }

    public ITransitionHandler? FindHandler(EnumTransitionType value) => HandlerLookup.GetValueOrDefault(value);
}
