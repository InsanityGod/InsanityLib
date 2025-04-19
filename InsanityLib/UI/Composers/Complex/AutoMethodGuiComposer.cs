using InsanityLib.Interfaces.UI;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Client;

namespace InsanityLib.UI.Composers.Complex
{
    public class AutoMethodGuiComposer : IAutoGuiComposer
    {
        public void ComposeObject(GuiComposer composer, IServiceProvider provider, MemberInfo member, object value)
        {
            var method = (MethodBase)value;
            var context = provider.GetService<IDialogContext>();
            
            var inputBounds = ElementBounds.FixedSize(300, 40); //TODO see about making this fill the screen instead
            context.Cursor.Y += inputBounds.fixedHeight;
            
            var buttonText = member?.GetHumanReadableName() ?? method.Name;
            var element = new GuiElementTextButton(composer.Api, buttonText, CairoFont.ButtonText(), CairoFont.ButtonPressedText(), () =>
            {
                method.AutoInvoke(provider, context.TargetObject);
                return true;
            } , inputBounds);
            composer.AddInteractiveElement(element, context.ExtendPath<string>(member));
        }

        public bool IsValidForCompose(Type type) => typeof(MethodBase).IsAssignableFrom(type);
    }
}
