using AutoConfigLib;
using ImGuiNET;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.UI.ImGuiTools.Components.Values
{
    public class StringComponent : ValueComponentBase
    {
        private string value;
        public uint MaxStringLength { get; init; }
        public StringComponent(ImGuiContext context) : base(context) 
        {
            value = context.Member.GetValue(context.TargetObject).AutoConvert<string>();
            
            MaxStringLength = 128;
            
            var stringLengthAttr = context.Member.GetCustomAttribute<StringLengthAttribute>();
            var maxLengthAttr = context.Member.GetCustomAttribute<MaxLengthAttribute>();
            
            if(stringLengthAttr != null && maxLengthAttr != null) MaxStringLength = (uint)Math.Min(stringLengthAttr.MaximumLength, maxLengthAttr.Length);
            else if(stringLengthAttr != null) MaxStringLength = (uint)stringLengthAttr.MaximumLength;
            else if(maxLengthAttr != null) MaxStringLength = (uint)maxLengthAttr.Length;
        }

        public override void RenderValue()
        {
            if(ImGui.InputText(Context.Label, ref value, MaxStringLength))
            {
                OnValueChanged(value);
            }
        }
    }
}
