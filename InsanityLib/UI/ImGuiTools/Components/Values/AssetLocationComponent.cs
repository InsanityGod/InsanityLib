using ImGuiNET;
using InsanityLib.Util;
using InsanityLib.Util.SpanUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace InsanityLib.UI.ImGuiTools.Components.Values
{
    public class AssetLocationComponent : ValueComponentBase<string>
    {
        public AssetLocationComponent(ImGuiContext context) : base(context)
        {
            
        }
        
        private AssetLocation location;

        public override void RenderValue()
        {
            if (ImGui.InputText(Context.Label, ref value, 128)) //TODO
            {
                //TODO dropdown search box for domain (to avoid bad interning)

                if (string.IsNullOrEmpty(value) || value == ":")
                {
                    value = string.Empty;
                    Context.TrySetValue(null, this);
                    return;
                }
                
                var components = value.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);

                location.Domain = components.Length > 1 ? components[0] : string.Empty;
                location.Path = components[^1];

                Context.TryAutoSetValue(location, this);
            }
        }

        protected override void OnValueChanged(object sender, PropertyChangedEventArgs args)
        {
            if(!Context.TryGetValue(out var obj)) return;

            if (obj is not null)
            {
                location = obj as AssetLocation;
                value = location.ToStringSimple();
            }
            else location ??= new AssetLocation(); //Initialize to avoid null reference exceptions

            Validate();

            value ??= string.Empty;
        }
    }
}
