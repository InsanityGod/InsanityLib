using ImGuiNET;
using InsanityLib.Extended.Enums;
using InsanityLib.Extensions;
using System;
using System.ComponentModel;
using Vintagestory.API.Client;
using Vintagestory.API.Util;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components.Values;

public class EnumComponent : ValueComponentBase<long>
{
    public EnumNameValueMapping Mapping { get; set; }
    
    private int index;
    public EnumComponent(ImGuiContext context) : base(context)
    {
        Mapping = new EnumNameValueMapping(context.ComposeType);
        UpdateValues();
    }

    private void UpdateValues()
    {
        if(Mapping is not null)
        {
            DisplayStr = Mapping.GetDisplayString(value);
            if(!Mapping.IsEnumFlag) index = Mapping.NumericValues.IndexOf(value);
        }
    }

    public string? DisplayStr { get; private set; }

    protected override void OnValueChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.OnValueChanged(sender, args);
        UpdateValues();
    }

    public override void RenderValue()
    {
        if (!Mapping.IsEnumFlag)
        {
            //TODO maybe add a combined enum description? (description of all enum values)
            if(ImGui.Combo(Context.Label, ref index, Mapping.Names, Mapping.Names.Length)) Context.TryAutoSetValue(Mapping.NumericValues[index], this);

            return;
        }

        if (ImGui.BeginCombo(Context.Label, DisplayStr))
        {
            for (int i = 0; i < Mapping.StrValues.Length; i++)
            {
                var flag = Mapping.NumericValues[i];
                var isSelected  = (value & flag) != 0;
                if (ImGui.Selectable($"{Mapping.Names[i]}##{Context.Id}-item-{flag}", isSelected))
                {
                    if (isSelected)
                    {
                        value &= ~flag;
                    }
                    else
                    {
                        value |= flag;
                    }

                    Context.TryAutoSetValue(value, this);
                }
            }
            ImGui.EndCombo();
        }
    }

    public override void Copy() => InsanityLibModSystem.GlobalServiceContainer.GetService<ICoreClientAPI>()!.Forms.SetClipboardText(Mapping.GetStringValue(value));

    public override void Paste()
    {
        int strIndex;
        var clipboard = InsanityLibModSystem.GlobalServiceContainer.GetService<ICoreClientAPI>()!.Forms.GetClipboardText();
        if (Mapping.IsEnumFlag)
        {
            var strings = clipboard.Split(", ");
            long flag = 0;
            foreach(var str in strings)
            {
                strIndex = Array.IndexOf(Mapping.StrValues, str);
                if(strIndex == -1) return;
                flag |= Mapping.NumericValues[strIndex];
            }

            Context.TryAutoSetValue(flag, this);
        }
        else
        {
            strIndex = Array.IndexOf(Mapping.StrValues, clipboard);
            if(strIndex != -1) Context.TryAutoSetValue(Mapping.NumericValues[strIndex] ,this);
        }
    }
}
