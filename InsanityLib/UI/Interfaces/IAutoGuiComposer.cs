using System;
using System.Reflection;
using Vintagestory.API.Client;

namespace InsanityLib.UI.Interfaces;

public interface IAutoGuiComposer<in T> : IAutoGuiComposer
{
    public void Compose(GuiComposer composer, IServiceProvider provider, MemberInfo member, T value);

    void IAutoGuiComposer.ComposeObject(GuiComposer composer, IServiceProvider provider, MemberInfo member, object value) => Compose(composer, provider, member, (T)value);

    bool IAutoGuiComposer.IsValidForCompose(Type type) => typeof(T).IsAssignableFrom(type);
}

public interface IAutoGuiComposer
{
    public void ComposeObject(GuiComposer composer, IServiceProvider provider, MemberInfo member, object value);

    public bool IsValidForCompose(Type type);
}
