using InsanityLib.Util.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace InsanityLib.Extended.Transitions;

public interface ITransitionHandler : IInitialize
{
    float DefaultTransitionSpeedMul { get; }
    
    AssetLocation TransitionCode { get; init; }
    
    EnumTransitionType TransitionType { get; init; }

    void AddProccessIntoInfoToHandbook(ICoreClientAPI capi, List<RichTextComponentBase> components, ClearFloatTextComponent verticalSpace, ActionConsumable<string> openDetailPageFor, TransitionableProperties prop);
    
    void AppendAppendPerishableInfoText(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, TransitionState state, bool nowSpoiling);

    float GetTransitionRateMul(IWorldAccessor world, ItemSlot inSlot, float currentResult);

    void IInitialize.Initialize(IServiceProvider serviceProvider)
    {
        //Optional
    }

    void LoadAttributes(JsonObject attributes)
    {
        //Optional
    }

    void PostOnTransitionNow(CollectibleObject collectible, ItemSlot slot, TransitionableProperties props, ref ItemStack result)
    {
        //Optional
    }
}
