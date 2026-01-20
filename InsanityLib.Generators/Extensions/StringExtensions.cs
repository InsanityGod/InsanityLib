using System.CodeDom.Compiler;
using System.Text;

namespace InsanityLib.Generators.Extensions;

public static class StringExtensions
{
    public static string ToLiteral(this string value)
    {
        var sb = new StringBuilder(value.Length + 2);
        sb.Append('"');
    
        foreach (char c in value)
        {
            switch (c)
            {
                case '"':  sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n");  break;
                case '\r': sb.Append("\\r");  break;
                case '\t': sb.Append("\\t");  break;
                default:
                    if (c < 32)
                    {
                        sb.Append("\\u").Append(((int)c).ToString("X4"));
                    }
                    else sb.Append(c);
                    break;
            }
        }
    
        sb.Append('"');
        return sb.ToString();
    }

    public static void WriteMultiLine(this IndentedTextWriter writer, string multiLineString)
    {
        var lines = multiLineString.Split(
            ["\r\n", "\r", "\n"],
            StringSplitOptions.RemoveEmptyEntries
        );

        foreach (var line in lines)
        {
            writer.WriteLine(line);
        }
    }
}
