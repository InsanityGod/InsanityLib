using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Extensions;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.ComponentModel.Design;

namespace InsanityLib.Generators;

public sealed partial class ModSystemGenerator
{
    private bool HasDisposalLogic;

    public void GenerateAutoDisposeMethod(IndentedTextWriter writer, GeneratorContext info)
    {
        GenerateDisposalMethodList(writer, info);

       writer.WriteMultiLine("""
        /// <summary>
        /// Method for automatically disposing stuff (based on attributes and stuff registered in other auto methods)
        /// </summary>
        """);
        //info.Compilation.GetSymbolsWithAttribute(IndentedTextWriter)
        using (new BlockContext("protected void AutoDispose()").Use(writer))
        {
            if (hasPatches)
            {
                writer.WriteLine($"""new Harmony("{info.ModID}").UnpatchAll("{info.ModID}");""");
            }
            
            foreach((var symbol, var _) in info.Compilation.GetSymbolsWithAttribute("InsanityLib.Generators.Attributes.AutoClearAttribute"))
            {
                writer.WriteLine($"{symbol.GetStaticMemberPath()}?.Clear();");
            }
            if (HasDisposalLogic)
            {
                writer.WriteMultiLine("""
                if(_api is null) return;
                    
                foreach (var operation in DisposalOperations.OrderBy(operation => operation.ExecutionOrder))
                {
                    try
                    {
                        if((operation.Side & _api.Side) == 0) continue;
                        if (!operation.MayRunTwice && operation.Side == EnumAppSide.Universal && _api is ICoreClientAPI capi && capi.IsSinglePlayer) continue;
                        operation.Action(ServiceContainer);
                    }
                    catch (Exception ex)
                    {
                        Mod.Logger.Error("Something went wrong during disposal logic: {0}", ex);
                    }
                }
                """);
            }
        }
    }

    public void GenerateDisposalMethodList(IndentedTextWriter writer, GeneratorContext info)
    {
        var matches = info.Compilation.GetSymbolsWithAttribute("InsanityLib.Generators.Attributes.DisposalLogicAttribute").ToList();
        if (!matches.Any()) return;
        HasDisposalLogic = true;

        writer.WriteLine("private static (int ExecutionOrder, EnumAppSide Side, bool MayRunTwice, Action<IServiceProvider> Action)[] DisposalOperations = [");
        writer.Indent++;

        bool first = true;
        foreach((var symbol, var attr) in matches)
        {
            if(symbol is not IMethodSymbol methodSymbol) return;
            if(!first) writer.WriteLine(",");
            first = false;

            writer.Write("(");
            writer.WriteLiteral(attr.NamedArguments.GetArgument("ExecutionOrder", 0));
            writer.Write(", (EnumAppSide) ");
            writer.WriteLiteral(attr.NamedArguments.GetArgument("Side", 3));
            writer.Write(", ");
            writer.WriteLiteral(attr.NamedArguments.GetArgument("MayRunTwice", false));
            writer.Write(", serviceProvider => ");

            writer.WriteCall(methodSymbol);

            writer.Write(")");
        }
        writer.WriteLine();
        
        writer.Indent--;
        writer.WriteLine("];");
        writer.WriteLine();
    }

    //public static (int ExecutionOrder, int Side, bool MayRunTwice, Action<IServiceProvider>)[] DisposalOperations;
}