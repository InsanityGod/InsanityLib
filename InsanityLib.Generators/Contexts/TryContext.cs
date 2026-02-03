using InsanityLib.Generators.Interfaces;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators.Contexts;

public struct TryContext : IWriteableContainer
{

    internal CatchContext[] CatchContexts;

    public readonly void WriteStart(IndentedTextWriter writer)
    {
        writer.WriteLine("try");
        writer.WriteLine("{");
        writer.Indent++;
    }

    public readonly void WriteEnd(IndentedTextWriter writer)
    {
        writer.Indent--;
        writer.WriteLine("}");

        if(CatchContexts is null)
        {
            writer.WriteLine("catch");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine("// Ignore");
            writer.Indent--;
            writer.WriteLine("}");
        }
        else foreach(var context in CatchContexts) context.Write(writer);
    }

    public static TryContext Catch(params CatchContext[] catchContexts) => new() { CatchContexts = catchContexts };
}
