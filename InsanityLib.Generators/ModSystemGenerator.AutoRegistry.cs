using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Enums;
using InsanityLib.Generators.Extensions;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;

namespace InsanityLib.Generators;

public sealed partial class ModSystemGenerator
{
    private const string AutoRegistryAttributeFullName = "InsanityLib.Generators.Attributes.AutoRegistryNameAttribute";

    private static AutoRegistryNameContext FromAttribute(AttributeData attr)
    {
        var schema = attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string s
            ? s
            : "{modid}:{name}";
    
        var naming = attr.ConstructorArguments.Length > 1 && attr.ConstructorArguments[1].Value != null
            ? (ENamingConvention)(int)attr.ConstructorArguments[1].Value
            : ENamingConvention.PascalCase;
    
        string[] removePrefix = null;
        string[] removeSuffix = null;
    
        if (attr.ConstructorArguments.Length > 2 && !attr.ConstructorArguments[2].IsNull)
        {
            removePrefix = ExtractStringArray(attr.ConstructorArguments[2]);
        }
    
        if (attr.ConstructorArguments.Length > 3 && !attr.ConstructorArguments[3].IsNull)
        {
            removeSuffix = ExtractStringArray(attr.ConstructorArguments[3]);
        }
    
        foreach (var named in attr.NamedArguments)
        {
            switch (named.Key)
            {
                case "RemovePrefix":
                    removePrefix = ExtractStringArray(named.Value);
                    break;
    
                case "RemoveSuffix":
                    removeSuffix = ExtractStringArray(named.Value);
                    break;
    
                case "NamingConvention":
                    naming = (ENamingConvention)(int)named.Value.Value;
                    break;
    
                case "Scheme":
                    schema = named.Value.Value as string ?? schema;
                    break;
            }
        }
    
        return new AutoRegistryNameContext
        {
            Schema = schema,
            NamingConvention = naming,
            RemovePrefix = removePrefix,
            RemoveSuffix = removeSuffix
        };
    }
    private static string[] ExtractStringArray(TypedConstant constant)
    {
        if (constant.IsNull || constant.Values.IsDefaultOrEmpty) return null;
    
        var list = new List<string>(constant.Values.Length);
    
        foreach (var v in constant.Values)
        {
            if (v.Value is string s)
                list.Add(s);
        }
    
        return list.ToArray();
    }

    public AutoRegistryNameContext AutoRegistryNameContext { get; set; }

    private static AutoRegistryNameContext GetNameContext(ISymbol symbol)
    {
        var attr = symbol
            .GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == AutoRegistryAttributeFullName);
        
        if(attr is not null)
        {
            return FromAttribute(attr);
        }

        return null;
    }

    private static string TransformName(string name, AutoRegistryNameContext context)
    {
        // Remove prefixes
        if (context.RemovePrefix is not null)
        {
            foreach (var prefix in context.RemovePrefix)
            {
                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(prefix.Length);
                    break;
                }
            }
        }
    
        // Remove suffixes
        if (context.RemoveSuffix is not null)
        {
            foreach (var suffix in context.RemoveSuffix)
            {
                if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.Length - suffix.Length);
                    break;
                }
            }
        }
    
        return ApplyNaming(name, context.NamingConvention);
    }

    private static string ApplyNaming(string input, ENamingConvention convention)
    {
        if(string.IsNullOrWhiteSpace(input)) return input;
        return convention switch
        {
            ENamingConvention.PascalCase => char.ToUpperInvariant(input[0]) + input.Substring(1),
            ENamingConvention.CamelCase => char.ToLowerInvariant(input[0]) + input.Substring(1),
            ENamingConvention.LowerCase => input.ToLowerInvariant(),
            ENamingConvention.UpperCase => input.ToUpperInvariant(),
            ENamingConvention.SnakeCase => ToSeparated(input, "_"),
            ENamingConvention.KebabCase => ToSeparated(input, "-"),
            _ => input
        };
    }

    private static string ToSeparated(string input, string separator)
    {
        var parts = Regex
            .Matches(input, @"[A-Z]?[a-z]+|[A-Z]+(?![a-z])|\d+")
            .Cast<Match>()
            .Select(m => m.Value.ToLowerInvariant());
    
        return string.Join(separator, parts);
    }

    public string GetAutoRegistryName(GeneratorContext info, INamedTypeSymbol symbol)
    {
        var context = GetNameContext(symbol) ?? AutoRegistryNameContext;

        return context.Schema
            .Replace("{modid}", info.ModID)
            .Replace("{name}", TransformName(symbol.Name, context));
    }


    public void GenerateAutoRegistryMethod(IndentedTextWriter writer, GeneratorContext info)
    {
        AutoRegistryNameContext = GetNameContext(info.Compilation.Assembly) ?? new AutoRegistryNameContext()
        {
            Schema = "{modid}:{name}"
        };

        writer.WriteMultiLine("""
        /// <summary>
        /// Automatically registers implementations of the following to the class registry: <see cref="Vintagestory.API.Common.Item" />, <see cref="Vintagestory.API.Common.Block" />, <see cref="Vintagestory.API.Common.CollectibleBehavior" />, <see cref="Vintagestory.API.Common.BlockBehavior" />, <see cref="Vintagestory.API.Common.BlockEntity" />, <see cref="Vintagestory.API.Common.BlockEntityBehavior" /> and <see cref="InsanityLib.Extended.Transitions.ITransitionHandler" />.<br/>
        /// Will also automatically register classes marked with <see cref="InsanityLib.Generators.Attributes.AssetCategoryAttribute" />.<br/>
        /// Naming Scheme: "modid:classname"
        /// </summary>
        """);

        using(new BlockContext("protected void AutoRegistry(ICoreAPI api)").Use(writer))
        {
            //Base game
            foreach(var item in info.Compilation.GetAllConcrete("Vintagestory.API.Common.Item"))
            {
                writer.WriteLine($"""api.RegisterItemClass("{GetAutoRegistryName(info, item)}", typeof({item.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));""");
            }

            foreach(var block in info.Compilation.GetAllConcrete("Vintagestory.API.Common.Block"))
            {
                writer.WriteLine($"""api.RegisterBlockClass("{GetAutoRegistryName(info, block)}", typeof({block.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));""");
            }

            var blockBehaviorSymbol = info.Compilation.GetTypeByMetadataName("Vintagestory.API.Common.BlockBehavior");
            foreach(var behavior in info.Compilation.GetAllConcrete("Vintagestory.API.Common.CollectibleBehavior"))
            {
                if (behavior.DerivesFrom(blockBehaviorSymbol))
                {
                    writer.WriteLine($"""api.RegisterBlockBehaviorClass("{GetAutoRegistryName(info, behavior)}", typeof({behavior.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));""");
                }
                else
                {
                    writer.WriteLine($"""api.RegisterCollectibleBehaviorClass("{GetAutoRegistryName(info, behavior)}", typeof({behavior.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));""");
                }
            }

            foreach(var blockEntity in info.Compilation.GetAllConcrete("Vintagestory.API.Common.BlockEntity"))
            {
                writer.WriteLine($"""api.RegisterBlockEntityClass("{GetAutoRegistryName(info, blockEntity)}", typeof({blockEntity.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));""");
            }

            foreach(var blockEntityBehavior in info.Compilation.GetAllConcrete("Vintagestory.API.Common.BlockEntityBehavior"))
            {
                writer.WriteLine($"""api.RegisterBlockEntityBehaviorClass("{GetAutoRegistryName(info, blockEntityBehavior)}", typeof({blockEntityBehavior.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));""");
            }
            
            foreach(var entity in info.Compilation.GetAllConcrete("Vintagestory.API.Common.Entities.Entity"))
            {
                writer.WriteLine($"""api.RegisterEntity("{GetAutoRegistryName(info, entity)}", typeof({entity.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));""");
            }

            foreach(var entityBehavior in info.Compilation.GetAllConcrete("Vintagestory.API.Common.Entities.EntityBehavior"))
            {
                writer.WriteLine($"""api.RegisterEntityBehaviorClass("{GetAutoRegistryName(info, entityBehavior)}", typeof({entityBehavior.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));""");
            }

            //InsanityLib
            foreach(var blockEntity in info.Compilation.GetAllConcreteImplementations("InsanityLib.Extended.Transitions.ITransitionHandler"))
            {
                //AssetLocations are loaded in lower case when parsed from json, so we need to ensure the code is registered in lower case to avoid issues with case sensitivity
                writer.WriteLine($"""InsanityLib.Extended.Transitions.CustomTransition.RegisterHandler<{blockEntity.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}>(new("{info.ModID}", "{blockEntity.Name.ToLower()}"));""");
            }

            foreach((var type, var attr) in assetCategoryAttributes)
            {
                writer.Write("RegisterOrGetAssetCategory(api, ");
                writer.WriteLiteral(attr.ConstructorArguments[0].Value);
                writer.Write(", ");
                writer.WriteLiteral(attr.ConstructorArguments[1].Value);
                writer.Write(", ");
                writer.WriteLiteral(attr.ConstructorArguments[2].Value, info.Compilation.GetTypeByMetadataName("Vintagestory.API.Common.EnumAppSide"), false);
                writer.WriteLine($", typeof({type.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));");
            }
        }
    }
}