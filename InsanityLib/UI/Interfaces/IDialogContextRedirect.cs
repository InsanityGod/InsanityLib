using System;
using Vintagestory.API.MathTools;

namespace InsanityLib.UI.Interfaces;

public interface IDialogContextRedirect : IDialogContext
{
    public IDialogContext Context { get; }

    object IDialogContext.TargetObject => Context.TargetObject;

    bool IDialogContext.IsEditable => Context.IsEditable;

    Vec2d IDialogContext.Cursor => Context.Cursor;

    void IDialogContext.RegisterAfterComposeCallback(Action action) => Context.RegisterAfterComposeCallback(action);


    object IServiceProvider.GetService(Type serviceType)
    {
        if(serviceType.IsInstanceOfType(this)) return this;
        return Context.GetService(serviceType);
    }
}
