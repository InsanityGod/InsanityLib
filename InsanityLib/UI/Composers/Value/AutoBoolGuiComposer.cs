using InsanityLib.Interfaces.UI;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;

namespace InsanityLib.UI.Composers.Value
{
    public class AutoBoolGuiComposer : BaseValueGuiComposer<bool>
    {
        public override void ComposeValueRenderer(GuiComposer composer, IDialogContext context, MemberInfo member, bool value)
        {
            var inputBounds = ElementBounds.FixedSize(300, 40);
            if (member != null) inputBounds.FixedRightOf(composer.LastAddedElement.Bounds, GuiStyle.HalfPadding);
            context.Cursor.Y += inputBounds.fixedHeight;
            var element = new GuiElementSwitch(composer.Api, value => member.SetValue(value, context.TargetObject), inputBounds)
            {
                On = value,
                Enabled = context.IsMemberEditable(member)
            };
            composer.AddInteractiveElement(element, context.ExtendPath<string>(member));
        }
    }
}
