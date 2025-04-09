using Cairo;
using InsanityLib.Interfaces.UI;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace InsanityLib.UI.Composers.value
{
    public class AutoStringGuiComposer : IAutoGuiComposer<string>
    {

        public void Compose(GuiComposer composer, IServiceProvider provider, MemberInfo member, string value)
        {
            var dialogContext = provider.GetService<IDialogContext>();
            if (!dialogContext.IsMemberVisible(member)) return;

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

                Compose(composer, provider, null, member.GetHumanReadableName());
                dialogContext.Curor.Y = y;
            }

            if (dialogContext.IsMemberEditable(member))
            {
                //TODO a way to specify text area as well
                var bounds = ElementBounds.FixedSize(300, 40);

                if (member != null) bounds.FixedRightOf(composer.LastAddedElement.Bounds, GuiStyle.HalfPadding);
                dialogContext.Curor.Y += bounds.fixedHeight;
                var element = new GuiElementTextInput(provider.GetService<ICoreClientAPI>(), bounds, value => member.SetValue(value, dialogContext.TargetObject), CairoFont.TextInput());

                composer.AddInteractiveElement(element);

                if(!string.IsNullOrEmpty(value)) element.SetValue(value);
            }
            else if (!string.IsNullOrEmpty(value))
            {
                var font = CairoFont.WhiteSmallText();

                var extents = font.GetTextExtents(value);
                var bounds = new ElementBounds
                {
                    Alignment = EnumDialogArea.LeftMiddle,
                    fixedWidth = extents.Width,
                    fixedHeight = extents.Height,
                    BothSizing = ElementSizing.Fixed
                };
                dialogContext.Curor.Y += bounds.fixedHeight;
                composer.AddStaticText(value, font, bounds);
            }

            if (member != null) composer.EndChildElements();
        }
    }
}
