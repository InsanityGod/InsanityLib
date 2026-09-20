using HarmonyLib;
using IntegratedModManager.Config;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace InsanityLib.Auto.Config.IMM.Composers.Simple;

public sealed class FloatingPointComposer : IIMMComposer
{
    public bool CanWriteType(IMMComposerContext context, MemberInfo member, JsonContract contract, out bool requiresAdvanced)
    {
        var type = IMMComposerContext.GetNonNullableType(member, out requiresAdvanced);
        return !type.IsEnum && (type.IsFloatingPoint() || type == typeof(decimal));
    }

    public void Write(IMMComposerContext context, MemberInfo member, JsonContract contract, ImmAdvancedSchema? advanced)
    {
        var isNullable = IMMComposerContext.IsNullable(member);
        if(isNullable) ArgumentNullException.ThrowIfNull(advanced);

        double? min = null;
        double? max = null;
        double? step = null;

        if(member.GetCustomAttribute<RangeAttribute>() is { } rangeAttr)
        {
            if(rangeAttr.Minimum is not null) min = Convert.ToDouble(rangeAttr.Minimum);
            if(rangeAttr.Maximum is not null) max = Convert.ToDouble(rangeAttr.Maximum);
        }

        var isSlider = min is not null && max is not null && double.IsFinite(min.Value) && double.IsFinite(max.Value);
        if (isSlider)
        {
            step = 0.01;
            if((max - min) / step > 1000)
            {
                isSlider = false;
                step = null;
            }
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
            var entry = IMMConfigGenerator.TopLevelEntry(member, type);
            if (isSlider) //These fields are not supported unless it's a slider...?
            {
                entry.Min = min;
                entry.Max = max;
                entry.Step = step;
            }
            context.IMMConfig.Settings.Add(entry);
        }
    }
}
