using Cairo;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace InsanityLib.Extended.Transitions;

public class TransitionHandler : ITransitionHandler
{
    public AssetLocation TransitionCode { get; set; }
    public EnumTransitionType TransitionType { get; set; }

    public virtual void LoadAttributes(JsonObject attributes)
    {
        //Optional method
    }

    public virtual void PostOnTransitionNow(CollectibleObject collectible, ItemSlot slot, TransitionableProperties props, ref ItemStack result)
    {
        //Optional method
    }

    public virtual float GetTransitionRateMul(IWorldAccessor world, ItemSlot inSlot, float currentResult) => currentResult;

    public virtual float DefaultTransitionSpeedMul => 1;

    public virtual void AppendAppendPerishableInfoText(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, TransitionState state, bool nowSpoiling)
    {
        var transitionRate = inSlot.Itemstack.Collectible.GetTransitionRateMul(world, inSlot, TransitionType);
        float transitionLevel = state.TransitionLevel;
			float hoursLeft = (state.TransitionHours - (state.TransitionedHours - state.FreshHoursLeft)) / transitionRate;

        double hoursPerday = (double)world.Calendar.HoursPerDay;
        double hoursPerYear = world.Calendar.DaysPerYear * hoursPerday;
        if(transitionLevel > 0f && state.FreshHoursLeft <= 0 && transitionRate <= 0) //TODO dubble check this display code and see
        {
            dsc.AppendLine(Lang.Get($"{TransitionCode.Domain}:transition-{TransitionCode.Path}-progression", (int)Math.Round((double)(transitionLevel * 100f))));
        }
			else
        {
            if (transitionRate <= 0f)
			    {
			    	dsc.AppendLine(Lang.Get($"{TransitionCode.Domain}:transition-{TransitionCode.Path}"));
			    }
			    else if ((double)hoursLeft > hoursPerYear)
			    {
			    	dsc.AppendLine(Lang.Get($"{TransitionCode.Domain}:transition-{TransitionCode.Path}-duration-years", Math.Round((double)hoursLeft / hoursPerYear, 1)));
			    }
            else if ((double)hoursLeft > hoursPerday)
			    {
			    	dsc.AppendLine(Lang.Get($"{TransitionCode.Domain}:transition-{TransitionCode.Path}-duration-days", Math.Round((double)hoursLeft / hoursPerday, 1)));
			    }
			    else
			    {
			    	dsc.AppendLine(Lang.Get($"{TransitionCode.Domain}:transition-{TransitionCode.Path}-duration-hours", Math.Round((double)hoursLeft, 1)));
			    }
        }
    }

    public virtual void AddProccessIntoInfoToHandbook(ICoreClientAPI capi, List<RichTextComponentBase> components, ClearFloatTextComponent verticalSpace, ActionConsumable<string> openDetailPageFor, TransitionableProperties prop)
    {
			components.Add(verticalSpace);
			components.Add(new RichTextComponent(capi, Lang.Get($"{TransitionCode.Domain}:transition-{TransitionCode.Path}-handbook", prop.TransitionHours.avg) + "\n", CairoFont.WhiteSmallText().WithWeight(FontWeight.Bold)));

			components.Add(
            new ItemstackTextComponent(
                capi,
                prop.TransitionedStack.ResolvedItemstack,
                40.0,
                10.0,
                EnumFloat.Inline, 
                stack => openDetailPageFor(GuiHandbookItemStackPage.PageCodeForStack(stack))
            )
            {
                PaddingLeft = 2.0
			    }
        );
    }
}
