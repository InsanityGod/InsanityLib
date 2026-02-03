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

    public static void WriteCall(this IndentedTextWriter writer, IMethodSymbol method, bool hasInsanityLibDependency = false, bool nullsafety = true, string serviceProviderIdentifier = "serviceProvider")
    {
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (method is null) throw new ArgumentNullException(nameof(method));

        writer.WriteTarget(method, serviceProviderIdentifier, nullsafety);
        writer.Write(method.Name);
        writer.Write("(");

        for (int i = 0; i < method.Parameters.Length; i++)
        {
            if (i > 0) writer.Write(", ");
            WriteArgument(writer, method.Parameters[i], serviceProviderIdentifier, hasInsanityLibDependency);
        }

        writer.Write(")");
    }

    public static void WriteGetService(this IndentedTextWriter writer, ITypeSymbol type, bool hasInsanityLibDependency = false, string sp = "serviceProvider")
    {
        if (hasInsanityLibDependency)
        {
            writer.Write(sp);
            writer.Write(".GetService<");
            writer.Write(type.ToDisplayString(QualifiedEnoughFormat));
            writer.Write(">()");
            return;
        }

        writer.Write('(');
        writer.Write(type.ToDisplayString(QualifiedEnoughFormat));
        writer.Write(')');
        writer.Write(sp);
        writer.Write(".GetService(typeof(");
        writer.Write(type.ToDisplayString(QualifiedEnoughFormat));
        writer.Write("))");
    }

    public static void WriteTarget(this IndentedTextWriter writer, IMethodSymbol method, string sp, bool hasInsanityLibDependency = false, bool nullsafety = true)
    {
        if (method.IsStatic)
        {
            writer.Write(method.ContainingType.ToDisplayString(QualifiedEnoughFormat));
            writer.Write('.');
            return;
        }
        
        
        // instance method -> resolve containing type from IServiceProvider
        writer.WriteGetService(method.ContainingType, hasInsanityLibDependency, sp);
        if (nullsafety) writer.Write('?');
        writer.Write('.');
    }

    public static void WriteArgument(IndentedTextWriter writer, IParameterSymbol parameter, string sp, bool hasInsanityLibDependency = false)
    {
        if (parameter.Type.ToDisplayString(QualifiedEnoughFormat) == "System.IServiceProvider")
        {
            writer.Write(sp);
            return;
        }

        if (parameter.CanBeService())
        {
            writer.WriteGetService(parameter.Type, hasInsanityLibDependency, sp);
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

    public static bool CanBeService(this IParameterSymbol p)
    {
        var type = p.Type;
    
        // Exclude value types (structs, enums, primitives)
        if (type.IsValueType) return false;
    
        // Exclude string explicitly
        if (type.SpecialType == SpecialType.System_String) return false;
    
        return type.TypeKind switch
        {
            TypeKind.Interface => true,
            TypeKind.Class => true,
            _ => false
        };
    }

    public static void WriteLiteral(this IndentedTextWriter writer, object value, ITypeSymbol type = null, bool writeFullNameSpace = true)
    {
        if (value is null)
        {
            writer.Write("null");
            return;
        }

        if (type is INamedTypeSymbol enumType && enumType.TypeKind == TypeKind.Enum)
        {
            var enumFormat = writeFullNameSpace
                ? QualifiedEnoughFormat
                : SymbolDisplayFormat.MinimallyQualifiedFormat;

            var enumTypeName = enumType.ToDisplayString(enumFormat);

            var numericValue = Convert.ToInt64(value, CultureInfo.InvariantCulture);

            var member = enumType
                .GetMembers()
                .OfType<IFieldSymbol>()
                .FirstOrDefault(f => f.HasConstantValue && Convert.ToInt64(f.ConstantValue!, CultureInfo.InvariantCulture) == numericValue);

            if (member is not null)
            {
                writer.Write(enumTypeName);
                writer.Write('.');
                writer.Write(member.Name);
            }
            else
            {
                writer.Write('(');
                writer.Write(enumTypeName);
                writer.Write(')');
                writer.Write(numericValue);
            }

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