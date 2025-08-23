using InsanityLib.Interfaces.UI;
using InsanityLib.UI.Contexts;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using System;
using System.Reflection;
using Vintagestory.API.Client;

namespace InsanityLib.UI.Composers;

public abstract class BaseValueGuiComposer<T> : IAutoGuiComposer<T>
{
    public virtual bool ShouldHide(IDialogContext context, MemberInfo member, T value) => !context.IsMemberVisible(member);

    public virtual void Compose(GuiComposer composer, IServiceProvider provider, MemberInfo member, T value)
    {
        var context = provider.GetService<IDialogContext>();
        if(ShouldHide(context, member, value)) return;
        bool hasDescriptor = TryAddDescriptor(composer, context, member);

        ComposeValueRenderer(composer, context, member, value);

        if (hasDescriptor) composer.EndChildElements();
    }

    public abstract void ComposeValueRenderer(GuiComposer composer, IDialogContext context, MemberInfo member, T value);


    protected virtual bool TryAddDescriptor(GuiComposer composer, IDialogContext dialogContext, MemberInfo member)
    {
        if (member is null) return false;

        var y = dialogContext.Cursor.Y;
        if (y != 0) y += GuiStyle.HalfPadding;

        var bounds = new ElementBounds
        {
            Alignment = EnumDialogArea.None,
            BothSizing = ElementSizing.FitToChildren,
            fixedY = y
        };
        composer.BeginChildElements(bounds);

        typeof(string)
            .FindAutoGuiComposer()
            .ComposeObject(composer, new DescriptorContext(dialogContext, member), null, member.GetHumanReadableName());

        dialogContext.Cursor.Y = y;
        
        return true;
    }
}
