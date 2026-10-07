using HarmonyLib;
using IntegratedModManager.Config;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace InsanityLib.Auto.Config.IMM.Composers.Simple;

public sealed class FloatingPointComposer : IIMMComposer
{
    public readonly double[] Steps = [0.001d, 0.01d, 1d, 10d];

    public bool CanWriteType(IMMComposerContext context, MemberInfo member, JsonProperty? property, JsonContract contract, out bool requiresAdvanced)
    {
        var type = IMMComposerContext.GetNonNullableType(member, out requiresAdvanced);
        return !type.IsEnum && (type.IsFloatingPoint() || type == typeof(decimal));
    }

    public void Write(IMMComposerContext context, MemberInfo member, JsonProperty? property, JsonContract contract, ImmAdvancedSchema? advanced)
    {
        var isNullable = IMMComposerContext.IsNullable(member);
        if(isNullable) ArgumentNullException.ThrowIfNull(advanced);

        double? min = null;
        double? max = null;
        double? step = null;
        double? defaultValue = null;
        if(advanced?.InitialValue is JValue initialValue) defaultValue = Convert.ToDouble(initialValue.Value);

        if(member.GetCustomAttribute<RangeAttribute>() is { } rangeAttr)
        {
            if(rangeAttr.Minimum is not null) min = Convert.ToDouble(rangeAttr.Minimum);
            if(rangeAttr.Maximum is not null) max = Convert.ToDouble(rangeAttr.Maximum);
        }

        var isSlider = min is not null && max is not null && double.IsFinite(min.Value) && double.IsFinite(max.Value);
        if (isSlider)
        {
            for (int i = 0; i < Steps.Length; i++)
            {
                if(!IsStepValid(min!.Value, max!.Value, Steps[i], defaultValue)) continue;
                step = Steps[i];
            }

            if(step is null) isSlider = false;
        }
        var type = isSlider ? "Slider" : "Decimal";

        if(advanced is not null)
        {
            advanced.Type = type;
            advanced.Nullable = isNullable;
            if (isSlider) //These fields are not supported unless it's a slider...?
            {
                advanced.Min = min;
                advanced.Max = max;
                advanced.Step = step;
            }
        }
        else
        {
            var entry = IMMConfigGenerator.TopLevelEntry(property, member, type);
            if (isSlider) //These fields are not supported unless it's a slider...?
            {
                entry.Min = min;
                entry.Max = max;
                entry.Step = step;
            }
            context.IMMConfig.Settings.Add(entry);
        }
    }

    public static bool IsStepValid(double min, double max, double step, double? defaultValue)
    {
        double num = step;
		double num2 = (max - min) / num;

        bool isStepValid =
            double.IsFinite(num2) &&
            num2 > 0.0 &&
            num2 <= 10000.0 &&
            Math.Abs(num2 - Math.Round(num2)) <= 1E-06;
        
        if(!isStepValid) return false;
        
        if (defaultValue.HasValue)
        {
            double defaultStep = (defaultValue.Value - min) / step;

            if (!double.IsFinite(defaultStep) || defaultStep < 0.0 || defaultStep > num2 || Math.Abs(defaultStep - Math.Round(defaultStep)) > 1E-06)
            {
                return false;
            }
        }

        return true;
    }
}
