using InsanityLib.Generators.Extensions;
using InsanityLib.Generators.Interfaces;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators.Contexts;

public struct CatchContext(string exception) : IWriteable
{
    public readonly string _exception = exception;

    public WriteDelgate Content { get; set; }

    public readonly void Write(IndentedTextWriter writer)
    {
        writer.Write("catch");
        if (!string.IsNullOrEmpty(_exception))
        {
            writer.Write($" (Exception exception)");
        }
        writer.WriteLine();
        writer.WriteLine("{");
        writer.Indent++;
        Content?.Invoke(writer);
        writer.Indent--;
        writer.WriteLine("}");
    }

    public static WriteDelgate LogContent(string method, string format = null, string parameters = null) => writer =>
    {
        if(!string.IsNullOrEmpty(format))
        {
            if (!string.IsNullOrEmpty(parameters))
            {
                writer.WriteLine($"{method}({format.ToLiteral()}, {parameters});");
            }
            else writer.WriteLine($"{method}({format.ToLiteral()});");
        }
        else writer.WriteLine($"{method}(exception);");
    };

    public static CatchContext Log(string exception, string method, string format = null, string parameters = null) => new(exception)
    {
        Content = LogContent(method, format, parameters)
    };
}
