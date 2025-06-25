using InsanityLib.Enums;
using InsanityLib.Interfaces.UI;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
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
    public class AutoEnumGuiComposer : BaseValueGuiComposer<object> , IAutoGuiComposer
    {

        public override void ComposeValueRenderer(GuiComposer composer, IDialogContext context, MemberInfo member, object value)
        {
            if (!context.IsMemberEditable(member))
            {
                typeof(string).FindAutoGuiComposer().ComposeObject(composer, context, null, value.ToString());
                return;
            }

            var inputBounds = ElementBounds.FixedSize(300, 40);
            if (member is not null) inputBounds.FixedRightOf(composer.LastAddedElement.Bounds, GuiStyle.HalfPadding);
            context.Cursor.Y += inputBounds.fixedHeight;
            
            var mapper = new EnumNameValueMapping(value.GetType());
            
            var element = new GuiElementDropDown(
                composer.Api,
                mapper.StrValues,
                mapper.Names,
                0,
                (Code, IsSelected) =>
                {
                    if(!mapper.IsEnumFlag) member.SetValue(Enum.Parse(mapper.EnumType, Code), context.TargetObject);
                    else
                    {
                        var currentValue = (Enum)member.GetValue(context.TargetObject);
                        var flagValue = (Enum)Enum.Parse(mapper.EnumType, Code);

                        if (IsSelected)
                        {
                            // Add the flag
                            var updatedValue = Enum.ToObject(mapper.EnumType, Convert.ToInt64(currentValue) | Convert.ToInt64(flagValue));
                            member.SetValue(updatedValue, context.TargetObject);
                        }
                        else
                        {
                            // Remove the flag
                            var updatedValue = Enum.ToObject(mapper.EnumType, Convert.ToInt64(currentValue) & ~Convert.ToInt64(flagValue));
                            member.SetValue(updatedValue, context.TargetObject);
                        }
                        
                    }
                },
                inputBounds,
                CairoFont.TextInput(),
                mapper.IsEnumFlag
            );

            composer.AddInteractiveElement(element, context.ExtendPath<string>(member));
            element.listMenu.ComposeDynamicElements();
            context.RegisterAfterComposeCallback(() => element.SetSelectedValue(mapper.GetStringValues(Convert.ToInt64(value))));
        }

        public bool IsValidForCompose(Type type) => type.IsEnum;
    }
}
