using InsanityLib.Generators.Interfaces;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators.Contexts;

public struct BlockContext(string start) : IWriteableContainer
{
    public readonly void WriteStart(IndentedTextWriter writer)
    {
        writer.WriteLine(start);
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
