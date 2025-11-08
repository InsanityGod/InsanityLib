using HarmonyLib;
using InsanityLib.UI.Interfaces;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using System;
using System.Reflection;
using Vintagestory.API.Client;

namespace InsanityLib.UI.Composers.Value;

public class AutoNumberGuiComposer : BaseValueGuiComposer<object>, IAutoGuiComposer
{
    public bool IsValidForCompose(Type type) => AccessTools.IsNumber(type);

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
        GuiElementNumberInput element = null;
        var primaryType = member.GetPrimaryType();
        element = new GuiElementNumberInput(composer.Api, inputBounds, value =>
        {
            try
            {
                var newValue = Convert.ChangeType(value, primaryType);
                member.SetValue(newValue, context.TargetObject);
            }
            catch
            {
                element.SetValue(member.GetValue(context.TargetObject).ToString());
            }
        }, CairoFont.TextInput());

        
        composer.AddInteractiveElement(element, context.ExtendPath<string>(member));

        element.SetValue(value.ToString());
    }

}
