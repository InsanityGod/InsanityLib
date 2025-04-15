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
    public class AutoAssetLocationGuiComposer :  BaseValueGuiComposer<AssetLocation>
    {

        //public void ComposeOLD(GuiComposer composer, IServiceProvider provider, MemberInfo member, AssetLocation value)
        //{
        //    var dialogContext = provider.GetService<IDialogContext>();
        //    if (!dialogContext.IsMemberVisible(member)) return;
        //    var strValue = value == null ? null : (value.HasDomain() ? value.ToString() : value.Path);
        //
        //    if (dialogContext.IsMemberEditable(member))
        //    {
        //        var bounds = ElementBounds.FixedSize(300, 40);
        //
        //        if (member != null) bounds.FixedRightOf(composer.LastAddedElement.Bounds, GuiStyle.HalfPadding);
        //        dialogContext.Cursor.Y += bounds.fixedHeight;
        //
        //        var element = new GuiElementTextInput(provider.GetService<ICoreClientAPI>(), bounds, value => member.SetValue(value.ToAssetLocation(), dialogContext.TargetObject), CairoFont.TextInput());
        //
        //        composer.AddInteractiveElement(element);
        //
        //        if(!string.IsNullOrEmpty(strValue)) element.SetValue(strValue);
        //    }
        //    else if (!string.IsNullOrEmpty(strValue))
        //    {
        //        var font = CairoFont.WhiteSmallText();
        //        var util = new TextDrawUtil();
        //
        //        var textBounds = util.Lineize(CairoFont.FontMeasuringContext, strValue, double.MaxValue, 1, EnumLinebreakBehavior.None)[0].Bounds;
        //        var bounds = new ElementBounds
        //        {
        //            Alignment = EnumDialogArea.LeftMiddle,
        //            fixedWidth = textBounds.Width,
        //            fixedHeight = textBounds.Height,
        //            BothSizing = ElementSizing.Fixed
        //        };
        //        dialogContext.Cursor.Y += bounds.fixedHeight;
        //        composer.AddStaticText(strValue, font, bounds);
        //    }
        //}

        public override bool ShouldHide(IDialogContext context, MemberInfo member, AssetLocation value) => base.ShouldHide(context, member, value) || (!context.IsMemberEditable(member) && string.IsNullOrEmpty(value));

        public override void ComposeValueRenderer(GuiComposer composer, IDialogContext context, MemberInfo member, AssetLocation value)
        {
            var strValue = value == null ? null : (value.HasDomain() ? value.ToString() : value.Path);
            
            if (context.IsMemberEditable(member))
            {
                var inputBounds = ElementBounds.FixedSize(300, 40);

                if (member != null) inputBounds.FixedRightOf(composer.LastAddedElement.Bounds, GuiStyle.HalfPadding);
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
}
