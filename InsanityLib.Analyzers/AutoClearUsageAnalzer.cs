using InsanityLib.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace InsanityLib.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AutoClearUsageAnalzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Diagnostics.StaticOnlyAttribute, Diagnostics.AtributeRequiresTypeWithMethod];

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
        var autoClearAtr = context.Compilation.GetTypeByMetadataName("InsanityLib.Generators.Attributes");

        var foundAttr = attributes.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, autoClearAtr));
        if(foundAttr is null) return;

        if (!symbol.IsStatic)
        {
            var diagnostic = Diagnostic.Create(Diagnostics.StaticOnlyAttribute, foundAttr.ApplicationSyntaxReference.GetSyntax().GetLocation(), foundAttr.AttributeClass.Name);
            context.ReportDiagnostic(diagnostic);
        }

        if (symbol is IFieldSymbol field)
        {
            CheckClearMethod(context, foundAttr, field, field.Type);
        }
        else if (symbol is IPropertySymbol property)
        {
            CheckClearMethod(context, foundAttr, property, property.Type);
        }
    }

    private static void CheckClearMethod(SymbolAnalysisContext context, AttributeData foundAttr, ISymbol symbol, ITypeSymbol fieldType)
    {
        var clearMethod = FindClearMethod(fieldType);
  
        if (clearMethod is null || !context.IsAccessible(clearMethod))
        {
            var diagnostic = Diagnostic.Create(
                Diagnostics.AtributeRequiresTypeWithMethod,
                foundAttr.ApplicationSyntaxReference.GetSyntax().GetLocation(),
                foundAttr.AttributeClass.Name,
                symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                "Clear()"
            );
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static IMethodSymbol FindClearMethod(ITypeSymbol type)
    {
        // Check current type + ALL base types recursively
        var currentType = type;
        while (currentType is not null)
        {
            var clearMethod = currentType.GetMembers("Clear")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m => m.Parameters.Length == 0);
          
            if (clearMethod is not null)
                return clearMethod;
          
            currentType = currentType.BaseType;  // Walk up inheritance chain
        }
      
        // Check interfaces too
        return type.AllInterfaces
            .SelectMany(i => i.GetMembers("Clear").OfType<IMethodSymbol>())
            .FirstOrDefault(m => m.Parameters.Length == 0);
    }
}
