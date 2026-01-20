using InsanityLib.Generators.Interfaces;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators.Contexts;

public struct ForeachContext(string item, string enumerable) : IWriteableContainer
{
    public readonly void WriteStart(IndentedTextWriter writer)
    {
        writer.WriteLine($"foreach (var {item} in {enumerable})");
        writer.WriteLine("{");
        writer.Indent++;
    }

    public readonly void WriteEnd(IndentedTextWriter writer)
    {
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();
    }
}
