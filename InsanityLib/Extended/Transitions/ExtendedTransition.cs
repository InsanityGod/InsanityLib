using InsanityLib.Constants;
using InsanityLib.Extended.Enums;
using InsanityLib.Util;
using InsanityLib.Util.ContentFeatures;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace InsanityLib.Extended.Transitions;

public sealed class ExtendedTransition : ExtendedEnum
{
    public ExtendedTransition() : base(typeof(EnumTransitionType)) { }

    private readonly Dictionary<EnumTransitionType, ITransitionHandler> HandlerLookup = [];

    public void RegisterTransitionType(IServiceProvider provider, TransitionType transitionType)
    {
        if(OffsetLookup.ContainsKey(transitionType.Code.ToString())) return;
        
        if(!CustomTransition.ClassRegistry.TryGetValue(transitionType.Handler, out var transitionHandlerType))
        {
            provider.GetService<ILogger>()?.Error(Logging.ExecutionFailedTemplate, nameof(RegisterTransitionType), transitionType.Code, $"No such transitionHandler '{transitionType.Handler}'");
            return;
        }


        if (transitionHandlerType.AutoCreate(provider, true) is not ITransitionHandler handler)
        {
            provider.GetService<ILogger>()?.Error(Logging.ExecutionFailedTemplate, nameof(RegisterTransitionType), transitionType.Code, $"Could not instantiate '{transitionHandlerType.FullName}'");
            return;
        }

        try
        {
            //TODO see about making these not publicly accessible
            handler.TransitionType = (EnumTransitionType)currentOffset;
            handler.TransitionCode = transitionType.Code;
            handler.LoadAttributes(transitionType.Attributes);
        }
        catch(Exception ex)
        {
            provider.GetService<ILogger>()?.Error(Logging.ExecutionFailedTemplate, nameof(ITransitionHandler.LoadAttributes), transitionHandlerType.FullName, ex);
            return;
        }
        HandlerLookup[(EnumTransitionType)currentOffset] = handler;
        OffsetLookup[transitionType.Code.ToString()] = currentOffset++;
    }

    public ITransitionHandler FindHandler(EnumTransitionType value) => HandlerLookup.GetValueOrDefault(value);
}
