using InsanityLib.Interfaces;
using System;
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
    
    void AppendAppendPerishableInfoText(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, TransitionState state, bool nowSpoiling);
    
    string GetCreatedByLangKeyForHandbook(ICoreClientAPI capi, EnumTransitionType type);

    string GetProcessIntoStringForHandbook(ICoreClientAPI capi, TransitionableProperties prop, string displayTime, object[] extra);
    
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
