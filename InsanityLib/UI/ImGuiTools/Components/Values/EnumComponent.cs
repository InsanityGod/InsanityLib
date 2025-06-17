using Cairo;
using ImGuiNET;
using InsanityLib.Enums;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Util;

namespace InsanityLib.UI.ImGuiTools.Components.Values
{
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
            if(Mapping != null)
            {
                DisplayStr = Mapping.GetDisplayString(value);
                if(!Mapping.IsEnumFlag) index = Mapping.NumericValues.IndexOf(value);
            }
        }

        public string DisplayStr { get; private set; }

        protected override void OnValueChanged(object sender, PropertyChangedEventArgs args)
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
    }
}
