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

    internal static readonly DiagnosticDescriptor MustBeClassWithEmptyConstructor = new(
        id: "INSANITY005",
        title: "Attribute is only allowed for classes with an empty constructor",
        messageFormat: "Invalid usage of '{0}', '{1}' is not a class or does not have a public empty constructor",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        //TODO helpLinkUri
        isEnabledByDefault: true
    );

    internal static readonly DiagnosticDescriptor SignatureDoesNotMatch = new(
        id: "INSANITY006",
        title: "The method signature does not match expected arguments/parameters/generics/etc",
        messageFormat: "Invalid usage of '{0}', method '{1}' parameters do not match the expected signature, {2}",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        //TODO helpLinkUri
        isEnabledByDefault: true
    );

    internal static readonly DiagnosticDescriptor StaticOrModSystemClassMembersOnly = new(
        id: "INSANITY007",
        title: "Attribute is only allowed on static members or members of a ModSystem class",
        messageFormat: "Attribute '{0}' can only be applied to static members or members of a ModSystem class",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    internal static readonly DiagnosticDescriptor DuplicateMatch = new(
        id: "INSANITY008",
        title: "Duplicate match found",
        messageFormat: "This attribute has a duplicate match and will not work as intended, duplicate match: {0}",
        category: "Setup",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    //TODO maybe some warnings about AutoMethods not being called?
}
