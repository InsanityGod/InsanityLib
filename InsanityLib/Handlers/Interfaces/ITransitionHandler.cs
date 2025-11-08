using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace InsanityLib.Handlers.Interfaces;

public interface ITransitionHandler
{
    float DefaultTransitionSpeedMul { get; }
    AssetLocation TransitionCode { get; set; }
    EnumTransitionType TransitionType { get; set; }

    void AddProccessIntoInfoToHandbook(ICoreClientAPI capi, List<RichTextComponentBase> components, ClearFloatTextComponent verticalSpace, ActionConsumable<string> openDetailPageFor, TransitionableProperties prop);
    void AppendAppendPerishableInfoText(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, TransitionState state, bool nowSpoiling);
    float GetTransitionRateMul(IWorldAccessor world, ItemSlot inSlot, float currentResult);
    void LoadAttributes(JsonObject attributes);
    void PostOnTransitionNow(CollectibleObject collectible, ItemSlot slot, TransitionableProperties props, ref ItemStack result);
}
