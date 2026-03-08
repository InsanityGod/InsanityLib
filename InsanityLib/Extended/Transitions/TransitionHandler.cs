using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace InsanityLib.Extended.Transitions;

public class TransitionHandler : ITransitionHandler
{
    #pragma warning disable CS8618
    public AssetLocation TransitionCode { get; init; }
    #pragma warning restore CS8618

    public EnumTransitionType TransitionType { get; init; }

    public virtual float GetTransitionRateMul(IWorldAccessor world, ItemSlot inSlot, float currentResult) => currentResult;

    public virtual float DefaultTransitionSpeedMul => 1;

    public virtual void AppendAppendPerishableInfoText(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, TransitionState state, bool nowSpoiling)
    {
        var transitionRate = inSlot.Itemstack.Collectible.GetTransitionRateMul(world, inSlot, TransitionType);
        float transitionLevel = state.TransitionLevel;
		float hoursLeft = (state.TransitionHours - (state.TransitionedHours - state.FreshHoursLeft)) / transitionRate;

        double hoursPerday = (double)world.Calendar.HoursPerDay;
        double hoursPerYear = world.Calendar.DaysPerYear * hoursPerday;
        if(transitionLevel > 0f && state.FreshHoursLeft <= 0 && transitionRate <= 0)
        {
            dsc.AppendLine(Lang.Get($"{TransitionCode.Domain}:transition-{TransitionCode.Path}-progression", (int)Math.Round((double)(transitionLevel * 100f))));
        }
        else if (transitionRate <= 0f)
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

    public string GetProcessIntoStringForHandbook(ICoreClientAPI capi, TransitionableProperties prop, string displayTime, object[] extra) => Lang.Get($"{TransitionCode.Domain}:handbook-processesinto-transition-" + TransitionCode.Path + displayTime, extra);

    public string GetCreatedByLangKeyForHandbook(ICoreClientAPI capi, EnumTransitionType type) => $"{TransitionCode.Domain}:handbook-createdby-transition-{TransitionCode.Path}";
}
