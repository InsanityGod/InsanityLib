using System;
using System.Linq;
using System.Reflection;

namespace InsanityLib.Documentation;

public static class DocumentationUtil
{
    public static AssemblyDocumentationContext GetDocumentationContext(this Assembly assembly) => AssemblyDocumentationContext.GetForAssembly(assembly);

    
    public static MemberDocumentationContext? GetDocumentationContext(this MemberInfo member)
    {
        if(member.DeclaringType is null) return null;

        var memberContext = new MemberDocumentationContext(member.DeclaringType.Assembly.GetDocumentationContext(), member);
        memberContext.Initialize(InsanityLibModSystem.GlobalServiceContainer);

        return memberContext;
    }

    public static string? GetDocumentationMemberName(this MemberInfo member) => member switch
    {
        Type type => $"T:{type.FullName}",
        MethodInfo method => $"M:{method.DeclaringType?.FullName}.{method.Name}({string.Join(",", method.GetParameters().Select(p => p.ParameterType.FullName))})",
        PropertyInfo property => $"P:{property.DeclaringType?.FullName}.{property.Name}",
        FieldInfo field => $"F:{field.DeclaringType?.FullName}.{field.Name}",
        EventInfo eventInfo => $"E:{eventInfo.DeclaringType?.FullName}.{eventInfo.Name}",
        _ => null,
    };
}

