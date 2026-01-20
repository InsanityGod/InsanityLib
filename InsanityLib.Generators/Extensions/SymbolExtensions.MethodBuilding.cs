using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.Globalization;

namespace InsanityLib.Generators.Extensions;

static partial class SymbolExtensions
{
    public static readonly SymbolDisplayFormat QualifiedEnoughFormat = new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
    );

    public static void WriteCall(this IndentedTextWriter writer, IMethodSymbol method, bool nullsafety = true, string serviceProviderIdentifier = "serviceProvider")
    {
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (method is null) throw new ArgumentNullException(nameof(method));

        WriteTarget(writer, method, serviceProviderIdentifier, nullsafety);
        writer.Write(method.Name);
        writer.Write("(");

        for (int i = 0; i < method.Parameters.Length; i++)
        {
            if (i > 0) writer.Write(", ");
            WriteArgument(writer, method.Parameters[i], serviceProviderIdentifier);
        }

        writer.Write(")");
    }

    public static void WriteTarget(IndentedTextWriter writer, IMethodSymbol method, string sp, bool nullsafety = true)
    {
        if (method.IsStatic)
        {
            writer.Write(method.ContainingType.ToDisplayString(QualifiedEnoughFormat));
            writer.Write('.');
            return;
        }
        
        
        // instance method -> resolve containing type from IServiceProvider
        writer.Write('(');
        writer.Write(method.ContainingType.ToDisplayString(QualifiedEnoughFormat));
        writer.Write(')');
        writer.Write(sp);
        writer.Write(".GetService(typeof(");
        writer.Write(method.ContainingType.ToDisplayString(QualifiedEnoughFormat));
        writer.Write("))");
        if (nullsafety) writer.Write('?');
        writer.Write('.');
    }

    public static void WriteArgument(IndentedTextWriter writer, IParameterSymbol parameter, string sp)
    {
        if (CanBeService(parameter))
        {
            writer.Write('(');
            writer.Write(parameter.Type.ToDisplayString(QualifiedEnoughFormat));
            writer.Write(')');
            writer.Write(sp);
            writer.Write(".GetService(typeof(");
            writer.Write(parameter.Type.ToDisplayString(QualifiedEnoughFormat));
            writer.Write("))");
            return;
        }

        if (parameter.HasExplicitDefaultValue)
        {
            WriteLiteral(writer, parameter.ExplicitDefaultValue, parameter.Type);
            return;
        }

        // optional or required but not a service -> default(T)
        writer.Write("default(");
        writer.Write(parameter.Type.ToDisplayString(QualifiedEnoughFormat));
        writer.Write(')');
    }

    public static bool CanBeService(IParameterSymbol p) => p.Type.TypeKind switch
    {
        TypeKind.Interface => true,
        TypeKind.Class => true,
        _ => false
    };

    public static void WriteLiteral(this IndentedTextWriter writer, object? value, ITypeSymbol type = null)
    {
        if (value is null)
        {
            writer.Write("null");
            return;
        }

        switch (value)
        {
            case string s:
                writer.Write('"');
                writer.Write(s.Replace("\\", "\\\\").Replace("\"", "\\\""));
                writer.Write('"');
                break;

            case char c:
                writer.Write("'");
                writer.Write(c);
                writer.Write("'");
                break;

            case bool b:
                writer.Write(b ? "true" : "false");
                break;

            case int or long or short or byte or float or double or decimal:
                writer.Write(Convert.ToString(value, CultureInfo.InvariantCulture));
                break;

            default:

                writer.Write("default");
                if(type is null) break;

                writer.Write("(");
                writer.Write(type.ToDisplayString(QualifiedEnoughFormat));
                writer.Write(')');
                break;
        }
    }
}