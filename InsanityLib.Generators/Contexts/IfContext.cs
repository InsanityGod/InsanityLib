using InsanityLib.Generators.Interfaces;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators.Contexts;

public struct IfContext(string condition) : IWriteableContainer
{
    public readonly void WriteStart(IndentedTextWriter writer)
    {
        writer.WriteLine($"if ({condition})");
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
