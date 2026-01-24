using ImGuiNET;
using InsanityLib.Util;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components.Values;

public class FloatComponent : ValueComponentBase<float>
{
    public float MinPercentageValue { get; set; }
    public float MaxPercentageValue { get; set; }
    public bool IsPercentage { get; set; }
    public bool UsePreciseInput { get; set; }
    public string? FormatString { get; set;}

    public FloatComponent(ImGuiContext context) : base(context)
    {
        FormatString = context.Member!.GetCustomAttribute<DisplayFormatAttribute>()?.DataFormatString;
        IsPercentage = FormatString?.ToLower() == "p";
        if (IsPercentage)
        {
            FormatString = "%.2f%%";

            var rangeAttr = context.Member!.GetCustomAttribute<RangeAttribute>();
            MinPercentageValue = rangeAttr?.Minimum.AutoConvert<float>() * 100 ?? 0;
            MaxPercentageValue = rangeAttr?.Maximum.AutoConvert<float>() * 100 ?? 100;
        }
    }

    public override void RenderValue()
    {
        
        if (IsPercentage && !float.IsNegativeInfinity(MinPercentageValue) && !float.IsPositiveInfinity(MaxPercentageValue))
        {
            var percentageValue = value * 100;

            if(UsePreciseInput
                ? ImGui.InputFloat(Context.Label, ref percentageValue, 0, 0, FormatString)
                : ImGui.SliderFloat(Context.Label, ref percentageValue, MinPercentageValue, MaxPercentageValue, FormatString))
            {
               value = percentageValue / 100;
                Context.TryAutoSetValue(value, this);
            }
        }
        else
        {
            if(ImGui.InputFloat(Context.Label, ref value, 0, 0, FormatString))
            {
                Context.TryAutoSetValue(value, this);
            }
        }
    }

    public override void RenderContextMenuContent()
    {
        if (IsPercentage)
        {
            if (UsePreciseInput)
            {
                if (ImGui.MenuItem("Use Percentage Input"))
                {
                    UsePreciseInput = false;
                }
            }
            else
            {
                if (ImGui.MenuItem("Use Precise Input"))
                {
                    UsePreciseInput = true;
                }
            }
        }

        base.RenderContextMenuContent();
    }
}
