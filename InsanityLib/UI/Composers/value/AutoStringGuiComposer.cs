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
using Vintagestory.GameContent;

namespace InsanityLib.UI.Composers.value
{
    public class AutoStringGuiComposer : BaseValueGuiComposer<string>
    {

        public override bool ShouldHide(IDialogContext context, MemberInfo member, string value) => base.ShouldHide(context, member, value) || (!context.IsMemberEditable(member) && string.IsNullOrEmpty(value));

        //public void ComposeOLD(GuiComposer composer, IServiceProvider provider, MemberInfo member, string value)
        //{
        //    var dialogContext = provider.GetService<IDialogContext>();
        //    if (!dialogContext.IsMemberVisible(member)) return;
        //    
        //    bool hasDescriptor = false;
        //    if (dialogContext.IsMemberEditable(member))
        //    {
        //        hasDescriptor = TryAddDescriptor(composer, dialogContext, member);
        //        //TODO a way to specify text area as well
        //        var bounds = ElementBounds.FixedSize(300, 40);
        //
        //        if (member != null) bounds.FixedRightOf(composer.LastAddedElement.Bounds, GuiStyle.HalfPadding);
        //        dialogContext.Cursor.Y += bounds.fixedHeight;
        //        var element = new GuiElementTextInput(provider.GetService<ICoreClientAPI>(), bounds, value => member.SetValue(value, dialogContext.TargetObject), CairoFont.TextInput());
        //
        //        composer.AddInteractiveElement(element, dialogContext.ExtendPath<string>(member));
        //
        //        if(!string.IsNullOrEmpty(value)) element.SetValue(value);
        //    }
        //    else if (!string.IsNullOrEmpty(value))
        //    {
        //        hasDescriptor = TryAddDescriptor(composer, dialogContext, member);
        //        var font = CairoFont.WhiteSmallText();
        //
        //        var extents = font.GetTextExtents(value);
        //        var bounds = new ElementBounds
        //        {
        //            Alignment = EnumDialogArea.LeftMiddle,
        //            fixedWidth = extents.Width,
        //            fixedHeight = extents.Height,
        //            BothSizing = ElementSizing.Fixed
        //        };
        //        dialogContext.Cursor.Y += bounds.fixedHeight;
        //        composer.AddStaticText(value, font, bounds, dialogContext.ExtendPath<string>(member));
        //    }
        //
        //    if (hasDescriptor) composer.EndChildElements();
        //}

        public override void ComposeValueRenderer(GuiComposer composer, IDialogContext context, MemberInfo member, string value)
        {
            if (context.IsMemberEditable(member))
            {
                //TODO a way to specify text area as well
                var inputBounds = ElementBounds.FixedSize(300, 40);

                if (member != null) inputBounds.FixedRightOf(composer.LastAddedElement.Bounds, GuiStyle.HalfPadding);
                context.Cursor.Y += inputBounds.fixedHeight;
                var element = new GuiElementTextInput(context.GetService<ICoreClientAPI>(), inputBounds, value => member.SetValue(value, context.TargetObject), CairoFont.TextInput());

                composer.AddInteractiveElement(element, context.ExtendPath<string>(member));

                if(!string.IsNullOrEmpty(value)) element.SetValue(value);
                return;
            }

            var font = CairoFont.WhiteSmallText();

            var extents = font.GetTextExtents(value);
            var displayBounds = new ElementBounds
            {
                Alignment = EnumDialogArea.LeftMiddle,
                fixedWidth = extents.Width,
                fixedHeight = extents.Height,
                BothSizing = ElementSizing.Fixed
            };
            context.Cursor.Y += displayBounds.fixedHeight;
            composer.AddStaticText(value, font, displayBounds, context.ExtendPath<string>(member));
        }
    }
}
