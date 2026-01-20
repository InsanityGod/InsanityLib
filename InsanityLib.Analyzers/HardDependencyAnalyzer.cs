using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace InsanityLib.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HardDependencyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Diagnostics.HardDependencyMissing];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol,
            SymbolKind.NamedType,
            SymbolKind.Method,
            SymbolKind.Event,
            SymbolKind.Field,
            SymbolKind.Property
        );
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var hasDependencyDefined = context.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.HasInsanityLibDependency", out var dependency) && !string.IsNullOrEmpty(dependency);

        if (hasDependencyDefined) return;

        var symbol = context.Symbol;

        if (!HasHardDependency(symbol)) return;

        context.ReportDiagnostic(Diagnostic.Create(
            Diagnostics.HardDependencyMissing,
            symbol.Locations.FirstOrDefault(),
            symbol.Name
        ));
    }

    private static bool HasHardDependency(ISymbol symbol)
    {
        foreach (var attr in symbol.GetAttributes())
        {
            // Skip "compile-time only" attributes
            if (attr.AttributeClass is not null && IsInInsanityGeneratorsAttributes(attr.AttributeClass)) continue;
        
            if (ContainsInsanityLibType(attr.AttributeClass)) return true;
        
            foreach (var arg in attr.ConstructorArguments) if (ContainsInsanityLibType(arg.Type)) return true;
        }
    
        // Then switch by symbol kind
        return symbol switch
        {
            INamedTypeSymbol t => HasHardDependency(t),
            IMethodSymbol m => HasHardDependency(m),
            IEventSymbol e => ContainsInsanityLibType(e.Type),
            IFieldSymbol f => ContainsInsanityLibType(f.Type),
            IPropertySymbol p => ContainsInsanityLibType(p.Type),
            _ => false
        };
    }

    private static bool HasHardDependency(INamedTypeSymbol t)
    {
        if (ContainsInsanityLibType(t.BaseType)) return true;
    
        foreach (var i in t.Interfaces)
            if (ContainsInsanityLibType(i)) return true;
    
        foreach (var tp in t.TypeParameters)
            foreach (var c in tp.ConstraintTypes)
                if (ContainsInsanityLibType(c)) return true;
    
        return false;
    }
    
    private static bool HasHardDependency(IMethodSymbol m)
    {
        if (ContainsInsanityLibType(m.ReturnType)) return true;
    
        foreach (var p in m.Parameters)
            if (ContainsInsanityLibType(p.Type)) return true;
    
        foreach (var tp in m.TypeParameters)
            foreach (var c in tp.ConstraintTypes)
                if (ContainsInsanityLibType(c)) return true;
    
        return false;
    }

    private static bool ContainsInsanityLibType(ITypeSymbol type)
    {
        if(type is null) return false;

        // Handles generics, arrays, nullable, etc.
        if (type is INamedTypeSymbol named)
        {
            if (IsInsanityLibNamespace(named.ContainingNamespace)) return true;

            foreach (var arg in named.TypeArguments) if (ContainsInsanityLibType(arg)) return true;
        }

        if (type is IArrayTypeSymbol array) return ContainsInsanityLibType(array.ElementType);

        if (type is IPointerTypeSymbol pointer) return ContainsInsanityLibType(pointer.PointedAtType);

        return false;
    }


    private static bool IsInsanityLibNamespace(INamespaceSymbol ns)
    {
        while (ns is not null && !ns.IsGlobalNamespace)
        {
            if (ns.Name == "InsanityLib") return true;
    
            ns = ns.ContainingNamespace;
        }
    
        return false;
    }

    private static bool IsInInsanityGeneratorsAttributes(INamedTypeSymbol type)
    {
        // Check if the attribute type is inside InsanityLib.Generators.Attributes namespace
        var ns = type.ContainingNamespace;
        while (ns is not null && !ns.IsGlobalNamespace)
        {
            if (ns.ToDisplayString() == "InsanityLib.Generators.Attributes")
                return true;
    
            ns = ns.ContainingNamespace;
        }
        return false;
    }
}
