using Microsoft.CodeAnalysis;

namespace InsanityLib.Analyzers;

internal static class Diagnostics
{
    internal static readonly DiagnosticDescriptor MissingModId = new(
        id: "INSANITY001",
        title: "ModID MSBuild property is missing",
        messageFormat: "The project must define <ModID> and mark it as CompilerVisibleProperty",
        category: "Setup",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/InsanityGod/InsanityLib/wiki/Diagnostics#insanity001-modid-msbuild-property-is-missing"
    );

    internal static readonly DiagnosticDescriptor StaticOnlyAttribute = new(
        id: "INSANITY002",
        title: "Attribute is only allowed on static members",
        messageFormat: "Attribute '{0}' can only be applied to static members",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    internal static readonly DiagnosticDescriptor AtributeRequiresTypeWithMethod = new(
        id: "INSANITY003",
        title: "Attribute is only allowed for types with a specific method accessible",
        messageFormat: "Attribute '{0}' requires '{1}' to provide a '{2}' method that is accessible",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    internal static readonly DiagnosticDescriptor HardDependencyMissing = new(
        id: "INSANITY004",
        title: "InsanityLib is required as dependency to use an InsanityLib type in this way",
        messageFormat: "Usage of type from InsanityLib in this way requires InsanityLib dependency, either refactor the code or add InsanityLib as dependency",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        //TODO helpLinkUri
        isEnabledByDefault: true
    );
}
