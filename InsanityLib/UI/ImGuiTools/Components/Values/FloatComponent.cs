using ImGuiNET;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace InsanityLib.UI.ImGuiTools.Components.Values
{
    public class FloatComponent : ValueComponentBase<float>
    {
        public float MinPercentageValue { get; set; }
        public float MaxPercentageValue { get; set; }
        public bool IsPercentage { get; set; }
        public string FormatString { get; set;}

        public FloatComponent(ImGuiContext context) : base(context)
        {
            FormatString = context.Member.GetCustomAttribute<DisplayFormatAttribute>()?.DataFormatString;
            IsPercentage = FormatString?.ToLower() == "p";
            if (IsPercentage)
            {
                FormatString = "%.2f%%";

                var rangeAttr = context.Member.GetCustomAttribute<RangeAttribute>();
                MinPercentageValue = rangeAttr?.Minimum.AutoConvert<float>() * 100 ?? 0;
                MaxPercentageValue = rangeAttr?.Maximum.AutoConvert<float>() * 100 ?? 100;
            }
        }

        public override void RenderValue()
        {
            if (IsPercentage && MinPercentageValue != float.NegativeInfinity && MaxPercentageValue != float.PositiveInfinity)
            {
                var percentageValue = value * 100; //TODO context menu that allowes turning off percentage input mode
                if(ImGui.SliderFloat(Context.Label, ref percentageValue, MinPercentageValue, MaxPercentageValue, FormatString))
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
    }
}
