using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators;

public sealed partial class ModSystemGenerator
{
    (ISymbol Symbol, AttributeData Attribute)[] assetCategoryAttributes;
    (ISymbol Symbol, AttributeData Attribute)[] assetCategoryAttributesWithLoadMethod;

    public void GenerateAssetCategoryMethods(IndentedTextWriter writer, GeneratorContext info)
    {
        assetCategoryAttributes = [.. info.Compilation.GetSymbolsWithAttribute("InsanityLib.Generators.Attributes.AssetCategoryAttribute")];
        if (assetCategoryAttributes.Length > 0) GenerateRegisterAssetCategoryMethod(writer, info);

        foreach((var type, _) in assetCategoryAttributes)
        {
            writer.WriteLine($"partial void {GetPartialAssetCategoryMethodName(type)}(ICoreAPI api, AssetLocation origin, {type.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)} asset);");
        }
        writer.WriteLine();

        assetCategoryAttributesWithLoadMethod = [.. assetCategoryAttributes.Where(category => FindUserImplementation(info.ContainingType, category.Symbol) is not null)];
        if(assetCategoryAttributesWithLoadMethod.Length == 0) return;

        writer.WriteMultiLine("""
        /// <summary>
        /// Loads an assetCategory (should be called after the assets are loaded)
        /// </summary>
        """);
        using (new BlockContext("protected void LoadAssetCategory<T>(ICoreAPI api, string categoryCode, Action<ICoreAPI, AssetLocation, T> OnLoad)").Use(writer))
        using(new ForeachContext("(var origin, var asset)", """api.Assets.GetMany<T>(Mod.Logger, $"{categoryCode}/")""").Use(writer))
        {
            writer.WriteLine("OnLoad(api, origin, asset);");
        }

        writer.WriteMultiLine("""
        /// <summary>
        /// Loads all categories
        /// </summary>
        """);
        using (new BlockContext("protected void LoadAssetCategories(ICoreAPI api)").Use(writer))
        foreach((var type, var attr) in assetCategoryAttributesWithLoadMethod)
        {
            writer.WriteLine($"""LoadAssetCategory<{type.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}>(api, "{attr.ConstructorArguments[0].Value}", {GetPartialAssetCategoryMethodName(type)});""");
        }

    }

    private static string GetPartialAssetCategoryMethodName(ISymbol assetType) => $"On{assetType.Name}Loaded";

    private static IMethodSymbol FindUserImplementation(INamedTypeSymbol containingType, ISymbol assetType)
    {
        if(containingType is null) return null;
        var expectedName = GetPartialAssetCategoryMethodName(assetType);
    
        foreach (var member in containingType.GetMembers(expectedName))
        {
            if (member is not IMethodSymbol method) continue;
   

            if (method.Parameters.Length != 3) continue;
            if (method.Parameters[0].Type.Name != "ICoreAPI") continue;
            if (method.Parameters[1].Type.Name != "AssetLocation") continue;
            if (!SymbolEqualityComparer.Default.Equals(method.Parameters[2].Type, assetType)) continue;
    
            return method;
        }
    
        return null;
    }

    public void GenerateRegisterAssetCategoryMethod(IndentedTextWriter writer, GeneratorContext info)
    {
        writer.WriteMultiLine("""
        /// <summary>
        /// Will register the assetcategory using <see cref="InsanityLib.Extended.AssetCategories.ExtendedAssetCategory" > if InsanityLib is present and asssetType is supplied
        /// </summary>
        """);
        using(new BlockContext("protected AssetCategory RegisterOrGetAssetCategory(ICoreAPI api, string code, bool affectsGameplay, EnumAppSide sideType, Type assetType = null)").Use(writer))
        {
            writer.WriteLine("if(AssetCategory.categories.TryGetValue(code, out var result)) return result;");
            using(new IfContext($"assetType is not null{(info.HasInsanityLibDependency ? "" : """&& api.ModLoader.IsModEnabled("insanitylib")""")}").Use(writer))
            {
                writer.WriteLine("return new ExtendedAssetCategory(assetType, code, affectsGameplay, sideType);");
            }
            writer.WriteLine("return new AssetCategory(code, affectsGameplay, sideType);");
        }
    }

}