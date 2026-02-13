using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Extensions;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators;

public sealed partial class ModSystemGenerator
{
    public void GenerateUtility(IndentedTextWriter writer, GeneratorContext info)
    {
        writer.WriteMultiLine("""
        /// <summary>
        /// List of events that should be unregistered when disposing
        /// </summary>
        """);

        writer.WriteLine("public List<long> GameTickListenerIds { get; private set; } = new();");
        writer.WriteLine();
    }

}