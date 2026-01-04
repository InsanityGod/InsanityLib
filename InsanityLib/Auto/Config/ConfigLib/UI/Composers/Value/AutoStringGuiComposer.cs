using InsanityLib.Auto.Config.ConfigLib.UI.Interfaces;
using InsanityLib.Extensions;
using InsanityLib.Util;
using System.Reflection;
using Vintagestory.API.Client;

namespace InsanityLib.Auto.Config.ConfigLib.UI.Composers.Value;

public class AutoStringGuiComposer : BaseValueGuiComposer<string>
{

    public override bool ShouldHide(IDialogContext context, MemberInfo member, string value) => base.ShouldHide(context, member, value) || !context.IsMemberEditable(member) && string.IsNullOrEmpty(value);

    public override void ComposeValueRenderer(GuiComposer composer, IDialogContext context, MemberInfo member, string value)
    {
        if (context.IsMemberEditable(member))
        {
            //TODO a way to specify text area as well
            var inputBounds = ElementBounds.FixedSize(300, 40);

            if (member is not null) inputBounds.FixedRightOf(composer.LastAddedElement.Bounds, GuiStyle.HalfPadding);
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
