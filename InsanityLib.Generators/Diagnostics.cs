using Microsoft.CodeAnalysis;

namespace InsanityLib.Generators;

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
}
