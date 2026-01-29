using Microsoft.CodeAnalysis;

namespace InsanityLib.Generators.Contexts;

public class GeneratorContext(Compilation compilation, string root, string modID, string projectDir, bool hasInsanityLibDependency)
{
    public Compilation Compilation { get; } = compilation;

    public string Root { get; set; } = root;

    public string ModID { get; set; } = modID;

    public string ProjectDir { get; set; } = projectDir;

    public bool HasInsanityLibDependency { get; set; } = hasInsanityLibDependency;

    public INamedTypeSymbol ContainingType { get; set; }
}
