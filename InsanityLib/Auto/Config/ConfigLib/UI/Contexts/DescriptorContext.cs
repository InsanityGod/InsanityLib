using InsanityLib.Auto.Config.ConfigLib.UI.Interfaces;
using System;
using System.Reflection;

namespace InsanityLib.Auto.Config.ConfigLib.UI.Contexts;

public class DescriptorContext(IDialogContext context, MemberInfo member) : IDialogContextRedirect
{
    public IDialogContext Context { get; } = context;

    public MemberInfo Member { get; } = member ?? throw new ArgumentNullException(nameof(member));

    public string Path => $"{Context.Path}/@Descriptor";

    public string ExtendPath(MemberInfo member, Type type) => member is null ? $"{Path}/{Member.Name}" : throw new InvalidOperationException("Descriptor should not have memberInfo passed");

    public static string GetDescriptorPath(MemberContext memberContext, MemberInfo member) => $"{memberContext.Path}/@Descriptor/{member.Name}";
}
