using System.Reflection;

namespace InsanityLib.Documentation;

public interface IDocumentedAttribute
{
    string? Documentation(MemberInfo member);
}
