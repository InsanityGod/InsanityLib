using InsanityLib.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace InsanityLib.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AutoConfigUsageAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Diagnostics.StaticOnlyAttribute, Diagnostics.MustBeClassWithEmptyConstructor];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field, SymbolKind.Property);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var symbol = context.Symbol;
      
        var attributes = symbol.GetAttributes();
        var autoConfigAttr = context.Compilation.GetTypeByMetadataName("InsanityLib.Generators.Attributes.AutoConfigAttribute");

        var foundAttr = attributes.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, autoConfigAttr));
        if(foundAttr is null) return;

        if (!symbol.IsStatic)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.StaticOnlyAttribute, foundAttr.ApplicationSyntaxReference.GetSyntax().GetLocation(), foundAttr.AttributeClass.Name);
            context.ReportDiagnostic(diagnostic);
        }

        INamedTypeSymbol memberType = symbol.GetPrimaryType();

        if (memberType is null || memberType.TypeKind != TypeKind.Class ||
            !memberType.Constructors.Any(c => c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public))
        {
                        var diagnostic = Diagnostic.Create(
                Diagnostics.MustBeClassWithEmptyConstructor,
                foundAttr.ApplicationSyntaxReference.GetSyntax().GetLocation(),
                foundAttr.AttributeClass.Name,
                memberType?.Name ?? "<unknown>"
            );
            context.ReportDiagnostic(diagnostic);
        }
    }

}
