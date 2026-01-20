using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace InsanityLib.Generators.Extensions;

static partial class SymbolExtensions
{
    public static IEnumerable<(ISymbol Symbol, AttributeData Attribute)> GetSymbolsWithAttribute(
        this Compilation compilation,
        string attributeMetadataName)
    {
        var attributeSymbol = compilation.GetTypeByMetadataName(attributeMetadataName);
        if (attributeSymbol == null) yield break; // Attribute not found

        var myAssembly = compilation.Assembly;

        foreach (var type in compilation.GlobalNamespace.GetAllTypes())
        {
            if (!SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, myAssembly)) continue;

            // Check attribute on the type itself
            foreach (var attr in type.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeSymbol)) yield return (type, attr);
            }

            // Check members
            foreach (var member in type.GetMembers())
            {
                if (member.IsImplicitlyDeclared) continue;

                if (!SymbolEqualityComparer.Default.Equals(member.ContainingAssembly, myAssembly)) continue;

                foreach (var attr in member.GetAttributes())
                {
                    if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeSymbol)) yield return (member, attr);
                }
            }
        }
    }

    public static IEnumerable<INamedTypeSymbol> GetAllConcreteImplementations(this Compilation compilation, string interfaceName)
    {
        var myAssembly = compilation.Assembly;
        var interfaceSymbol = compilation.GetTypeByMetadataName(interfaceName);
        if(interfaceSymbol is null) yield break;
    
        foreach (var type in compilation.GlobalNamespace.GetAllTypes())
        {
            if (type.TypeKind != TypeKind.Class) continue;
            if (type.IsAbstract) continue;
            if (!SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, myAssembly)) continue;
            
            if (type.ImplementsInterface(interfaceSymbol)) yield return type;
        }
    }

    public static bool ImplementsInterface(this INamedTypeSymbol type, INamedTypeSymbol interfaceSymbol)
    {
        // Includes direct and indirect interfaces
        foreach (var iface in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface, interfaceSymbol))
            {
                return true;
            }
        }
        return false;
    }

    public static IEnumerable<INamedTypeSymbol> GetAllConcrete(this Compilation compilation, string typeName)
    {
        var myAssembly = compilation.Assembly;
        var blockEntitySymbol = compilation.GetTypeByMetadataName(typeName);
        if (blockEntitySymbol is null) yield break; // BlockEntity not found

        foreach (var type in compilation.GlobalNamespace.GetAllTypes())
        {
            if (type.TypeKind != TypeKind.Class) continue;
            if (type.IsAbstract) continue;
            if (!SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, myAssembly)) continue;
            
            if (type.DerivesFrom(blockEntitySymbol)) yield return type;
        }
    }

    public static bool DerivesFrom(this INamedTypeSymbol symbol, INamedTypeSymbol baseType)
    {
        var current = symbol.BaseType;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType)) return true;

            current = current.BaseType;
        }
        return false;
    }

    public static IEnumerable<INamedTypeSymbol> GetAllTypes(this INamespaceSymbol ns)
    {
        foreach (var type in ns.GetTypeMembers())
        {
            yield return type;
            foreach (var nested in type.GetTypeMembersRecursive())
            {
                yield return nested;
            }
        }

        foreach (var nestedNs in ns.GetNamespaceMembers())
        {
            foreach (var nestedType in nestedNs.GetAllTypes())
            {
                yield return nestedType;
            }
        }
    }

    public static string GetStaticMemberPath(this ISymbol symbol) => symbol.ToDisplayString(StaticMemberPathFormat);

    public static readonly SymbolDisplayFormat StaticMemberPathFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        memberOptions: SymbolDisplayMemberOptions.IncludeContainingType,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
    );

    public static IEnumerable<INamedTypeSymbol> GetTypeMembersRecursive(this INamedTypeSymbol type)
    {
        foreach (var nested in type.GetTypeMembers())
        {
            yield return nested;

            foreach (var inner in nested.GetTypeMembersRecursive())
            {
                yield return inner;
            }
        }
    }

    public static bool IsAccessible(this SymbolAnalysisContext context, IMethodSymbol method)
    {
        // Public methods are always accessible
        if (method.DeclaredAccessibility == Accessibility.Public)
            return true;
        
        // Internal methods accessible if in same assembly as analyzer context
        if (method.DeclaredAccessibility == Accessibility.Internal)
        {
            var fieldAssembly = ((IFieldSymbol)context.Symbol).ContainingAssembly;
            return SymbolEqualityComparer.Default.Equals(method.ContainingAssembly, fieldAssembly);
        }
        
        return false;  // Not public/internal
    }

    public static object GetArgument(this ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments, string name, object defaultValue = null)
    {
        var result = namedArguments
            .Where(a => a.Key == name)
            .Select(a => new { Exists = true, a.Value.Value })
            .SingleOrDefault();

        if(result is null) return defaultValue;
        return result.Value;
    }
}