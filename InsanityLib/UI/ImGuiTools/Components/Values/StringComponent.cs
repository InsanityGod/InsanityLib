using ImGuiNET;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace InsanityLib.UI.ImGuiTools.Components.Values;

public class StringComponent : ValueComponentBase<string>
{
    public uint MaxStringLength { get; init; } = 128;

    public StringComponent(ImGuiContext context) : base(context) 
    {
        var stringLengthAttr = context.Member.GetCustomAttribute<StringLengthAttribute>();
        var maxLengthAttr = context.Member.GetCustomAttribute<MaxLengthAttribute>();
        
        if(stringLengthAttr is not null && maxLengthAttr is not null) MaxStringLength = (uint)Math.Min(stringLengthAttr.MaximumLength, maxLengthAttr.Length);
        else if(stringLengthAttr is not null) MaxStringLength = (uint)stringLengthAttr.MaximumLength;
        else if(maxLengthAttr is not null) MaxStringLength = (uint)maxLengthAttr.Length;
    }

    protected override void OnValueChanged(object sender, PropertyChangedEventArgs args)
    {
        base.OnValueChanged(sender, args);
        value ??= string.Empty;
    }

    public override void RenderValue()
    {
        if(ImGui.InputText(Context.Label, ref value, MaxStringLength))
        {
            Context.TryAutoSetValue(value, this);
        }
    }
}
