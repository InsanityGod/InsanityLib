using InsanityLib.Generators.Contexts;
using Microsoft.CodeAnalysis;

namespace InsanityLib.Generators;

[Generator(LanguageNames.CSharp)]
public sealed partial class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var rootNameSpace = context.AnalyzerConfigOptionsProvider.Select(
            (options, _) => options.GlobalOptions.TryGetValue("build_property.RootNamespace", out var ns) ? ns : "UnknownRootNameSpace"
        );

        var projectDir = context.AnalyzerConfigOptionsProvider.Select(
            (options, _) => options.GlobalOptions.TryGetValue("build_property.ProjectDir", out var dir) ? dir : ""
        );

        var modID = context.AnalyzerConfigOptionsProvider
            .Select((options, _) => options.GlobalOptions.TryGetValue("build_property.ModID", out var modID) ? modID : null);
        
        var hasInsanityLibDependency = context.AnalyzerConfigOptionsProvider
            .Select((options, _) => options.GlobalOptions.TryGetValue("build_property.HasInsanityLibDependency", out var dependency) ? dependency : "not_specified");

        var additionalInfo = context.CompilationProvider
            .Combine(rootNameSpace)
            .Combine(modID)
            .Combine(projectDir)
            .Combine(hasInsanityLibDependency)
            .Select((x, _) => new GeneratorContext(
                x.Left.Left.Left.Left,             // Compilation
                x.Left.Left.Left.Right,            // Root
                x.Left.Left.Right,                 // ModID
                x.Left.Right,                      // ProjectDir
                !string.IsNullOrEmpty(x.Right)     // HasInsanityLibDependency as bool
            ));

        context.RegisterSourceOutput(additionalInfo, GenerateModSystem);

    }

    private static void GenerateModSystem(SourceProductionContext context, GeneratorContext info)
    {
        info.Context = context;
        if(ShouldFail(context, info)) return;

        var generator = new ModSystemGenerator();
        generator.GenerateModSystemFile(context, info);
    }

    private static bool ShouldFail(SourceProductionContext context, GeneratorContext info)
    {
        if (string.IsNullOrWhiteSpace(info.ModID))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.MissingModId,
                Location.None
            ));
            info.ModID = "unknown";
        }

        return false;
    }
}