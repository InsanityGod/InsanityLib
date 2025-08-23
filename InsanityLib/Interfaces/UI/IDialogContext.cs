using InsanityLib.Util;
using System;
using System.ComponentModel;
using System.Reflection;
using Vintagestory.API.MathTools;

namespace InsanityLib.Interfaces.UI;

public interface IDialogContext : IServiceProvider
{
    public object TargetObject { get; }

    public bool IsEditable { get; }

    public string Path { get; }

    public string ExtendPath<T>(MemberInfo member) => ExtendPath(member, typeof(T));
    public string ExtendPath(MemberInfo member, Type type) => $"{Path}/{member?.Name ?? type.Name}";

    public bool IsMemberEditable(MemberInfo member)
    {
        if(!IsEditable || member is null || member.GetCustomAttribute<ReadOnlyAttribute>()?.IsReadOnly == true) return false;
        if (member is PropertyInfo property) return property.CanWrite;
        return member is FieldInfo;
    }

    public bool IsMemberVisible(MemberInfo member)
    {
        if(member is null) return true;
        if (member is PropertyInfo property) return property.CanRead;
        if (member is MethodInfo method) return method.CanAutoInvoke(this);
        return member is FieldInfo;
    }

    public Vec2d Cursor { get; }

    public void RegisterAfterComposeCallback(Action action);

}
