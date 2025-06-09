using ImGuiNET;
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

        public override void RenderValue()
        {
            if (ImGui.InputText(Context.Label, ref value, 128))
            {
                var components = value.Split(new[] { ':' }, 2);

                //TODO validator/warning attribute for checking for valid matches
                Context.TryAutoSetValue(components.Length > 1 ? new AssetLocation(components[0], components[1]) : new AssetLocation(null, components[1]), this);
            }
        }

        protected override void OnValueChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnValueChanged(sender, args);
            value ??= new AssetLocation(string.Empty, string.Empty);
        }
    }
}
