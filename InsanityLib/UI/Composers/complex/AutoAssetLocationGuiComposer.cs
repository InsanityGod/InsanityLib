using Cairo;
using InsanityLib.Interfaces.UI;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace InsanityLib.UI.Composers.complex
{
    public class AutoAssetLocationGuiComposer : IAutoGuiComposer<AssetLocation>
    {

        public void Compose(GuiComposer composer, IServiceProvider provider, MemberInfo member, AssetLocation value)
        {
            var dialogContext = provider.GetService<IDialogContext>();
            if (!dialogContext.IsMemberVisible(member)) return;
            var strValue = value == null ? null : (value.HasDomain() ? value.ToString() : value.Path);

            if (member != null)
            {
                var y = dialogContext.Curor.Y;
                if (y != 0) y += GuiStyle.HalfPadding;

                var bounds = new ElementBounds
                {
                    Alignment = EnumDialogArea.None,
                    BothSizing = ElementSizing.FitToChildren,
                    fixedY = y
                };
                composer.BeginChildElements(bounds);

                typeof(string).FindAutoGuiComposer().ComposeObject(composer, provider, null, member.GetHumanReadableName());
                dialogContext.Curor.Y = y;
            }

            if (dialogContext.IsMemberEditable(member))
            {
                var bounds = ElementBounds.FixedSize(300, 40);

                if (member != null) bounds.FixedRightOf(composer.LastAddedElement.Bounds, GuiStyle.HalfPadding);
                dialogContext.Curor.Y += bounds.fixedHeight;

                var element = new GuiElementTextInput(provider.GetService<ICoreClientAPI>(), bounds, value => member.SetValue(value.ToAssetLocation(), dialogContext.TargetObject), CairoFont.TextInput());

                composer.AddInteractiveElement(element);

                if(!string.IsNullOrEmpty(strValue)) element.SetValue(strValue);
            }
            else if (!string.IsNullOrEmpty(strValue))
            {
                var font = CairoFont.WhiteSmallText();
                var util = new TextDrawUtil();

                var textBounds = util.Lineize(CairoFont.FontMeasuringContext, strValue, double.MaxValue, 1, EnumLinebreakBehavior.None)[0].Bounds;
                var bounds = new ElementBounds
                {
                    Alignment = EnumDialogArea.LeftMiddle,
                    fixedWidth = textBounds.Width,
                    fixedHeight = textBounds.Height,
                    BothSizing = ElementSizing.Fixed
                };
                dialogContext.Curor.Y += bounds.fixedHeight;
                composer.AddStaticText(strValue, font, bounds);
            }

            if (member != null) composer.EndChildElements();
        }
    }
}
