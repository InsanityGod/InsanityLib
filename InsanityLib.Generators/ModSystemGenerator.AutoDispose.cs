using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Extensions;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;

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
        
        using (new BlockContext("protected void AutoDispose()").Use(writer))
        {
            if (HasDisposalLogic)
            {
                writer.WriteMultiLine("""
                if(_api is not null && ServiceContainer is not null)
                {
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
                }
                """);
            }

            if (hasPatches)
            {
                writer.WriteLine($"""new Harmony("{info.ModID}").UnpatchAll("{info.ModID}");""");
            }
            
            foreach((var symbol, var _) in info.Compilation.GetSymbolsWithAttribute("InsanityLib.Generators.Attributes.AutoClearAttribute"))
            {
                writer.WriteLine($"{symbol.GetStaticMemberPath()}?.Clear();");
            }

            foreach((var _, var attr) in assetCategoryAttributes)
            {
                writer.Write("AssetCategory.categories.Remove(");
                writer.WriteLiteral(attr.ConstructorArguments[0].Value);
                writer.WriteLine(");");
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
            writer.Write(", ");
            writer.WriteLiteral(attr.NamedArguments.GetArgument("Side", 3), info.Compilation.GetTypeByMetadataName("Vintagestory.API.Common.EnumAppSide"), false);
            writer.Write(", ");
            writer.WriteLiteral(attr.NamedArguments.GetArgument("MayRunTwice", false));
            writer.Write(", serviceProvider => ");

            writer.WriteCall(methodSymbol, info.HasInsanityLibDependency);

            writer.Write(")");
        }
        writer.WriteLine();
        
        writer.Indent--;
        writer.WriteLine("];");
        writer.WriteLine();
    }
}