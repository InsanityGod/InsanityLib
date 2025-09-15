using InsanityLib.UI.Interfaces;
using InsanityLib.Util;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace InsanityLib.UI.Composers.Complex;

public class AutoAssetLocationGuiComposer :  BaseValueGuiComposer<AssetLocation>
{

    public override bool ShouldHide(IDialogContext context, MemberInfo member, AssetLocation value) => base.ShouldHide(context, member, value) || (!context.IsMemberEditable(member) && string.IsNullOrEmpty(value));

    public override void ComposeValueRenderer(GuiComposer composer, IDialogContext context, MemberInfo member, AssetLocation value)
    {
        string strValue = null;
        if (value is not null) strValue = value.HasDomain() ? value.ToString() : value.Path;
        
        if (context.IsMemberEditable(member))
        {
            var inputBounds = ElementBounds.FixedSize(300, 40);

            if (member is not null) inputBounds.FixedRightOf(composer.LastAddedElement.Bounds, GuiStyle.HalfPadding);
            context.Cursor.Y += inputBounds.fixedHeight;

            var element = new GuiElementTextInput(context.GetService<ICoreClientAPI>(), inputBounds, value => member.SetValue(value.ToAssetLocation(), context.TargetObject), CairoFont.TextInput());

            composer.AddInteractiveElement(element, context.ExtendPath<AssetLocation>(member));

            if(!string.IsNullOrEmpty(strValue)) element.SetValue(strValue);
            
            return;
        }
        
        var font = CairoFont.WhiteSmallText();
        var util = new TextDrawUtil();

        var displayBounds = util.Lineize(CairoFont.FontMeasuringContext, strValue, double.MaxValue, 1, EnumLinebreakBehavior.None)[0].Bounds;
        var bounds = new ElementBounds
        {
            Alignment = EnumDialogArea.LeftMiddle,
            fixedWidth = displayBounds.Width,
            fixedHeight = displayBounds.Height,
            BothSizing = ElementSizing.Fixed
        };
        context.Cursor.Y += bounds.fixedHeight;
        composer.AddStaticText(strValue, font, bounds, context.ExtendPath<AssetLocation>(member));
    }
}
