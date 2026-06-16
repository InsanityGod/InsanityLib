using InsanityLib.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace InsanityLib.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AutoNetworkMessageAttributeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Diagnostics.SignatureDoesNotMatch, Diagnostics.StaticOrModSystemClassMembersOnly];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var symbol = context.Symbol;

        var attributes = symbol.GetAttributes();
        var autoConfigAttr = context.Compilation.GetTypeByMetadataName("InsanityLib.Generators.Attributes.AutoNetworkMessageAttribute");

        var foundAttr = attributes.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, autoConfigAttr));
        if (foundAttr is null || symbol is not IMethodSymbol method) return;

        if(!method.ReturnsVoid)
        {
            InvalidAutoNetworkMesage(context, foundAttr, method);
        }

        var iServerPlayerType = context.Compilation.GetTypeByMetadataName("Vintagestory.API.Server.IServerPlayer");
        var parameters = method.Parameters;
        
        if(parameters.Length > 2 || parameters.Length < 1 || (parameters.Length == 2 && !parameters[0].Type.Equals(iServerPlayerType, SymbolEqualityComparer.Default)))
        {
            InvalidAutoNetworkMesage(context, foundAttr, method);
        }

        if (symbol.IsStatic) return;
        
        var modSystemClass = context.Compilation.GetTypeByMetadataName("Vintagestory.API.Common.ModSystem");
        if (symbol.ContainingType != null && symbol.ContainingType.DerivesFrom(modSystemClass))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Diagnostics.StaticOrModSystemClassMembersOnly, foundAttr.ApplicationSyntaxReference.GetSyntax().GetLocation(), foundAttr.AttributeClass.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static void InvalidAutoNetworkMesage(SymbolAnalysisContext context, AttributeData foundAttr, IMethodSymbol method)
    {
        //TODO fill in expected signature with actual method name and parameter types + names
        context.ReportDiagnostic(Diagnostic.Create(
            Diagnostics.SignatureDoesNotMatch,
            foundAttr.ApplicationSyntaxReference.GetSyntax().GetLocation(),
            foundAttr.AttributeClass.Name,
            method.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            "Expected signature: void MethodName([ServerSideOnly] IServerPlayer fromPlayer, AnyType packet)"
        ));
    }
}
