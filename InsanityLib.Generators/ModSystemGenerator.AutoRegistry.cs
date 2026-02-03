using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Extensions;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators;

public sealed partial class ModSystemGenerator
{


    public void GenerateAutoRegistryMethod(IndentedTextWriter writer, GeneratorContext info)
    {

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
                writer.WriteLine($"""api.RegisterItemClass("{info.ModID}:{item.Name}", typeof({item.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));""");
            }

            foreach(var block in info.Compilation.GetAllConcrete("Vintagestory.API.Common.Block"))
            {
                writer.WriteLine($"""api.RegisterBlockClass("{info.ModID}:{block.Name}", typeof({block.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));""");
            }

            var blockBehaviorSymbol = info.Compilation.GetTypeByMetadataName("Vintagestory.API.Common.BlockBehavior");
            foreach(var behavior in info.Compilation.GetAllConcrete("Vintagestory.API.Common.CollectibleBehavior"))
            {
                if (behavior.DerivesFrom(blockBehaviorSymbol))
                {
                    writer.WriteLine($"""api.RegisterBlockBehaviorClass("{info.ModID}:{behavior.Name}", typeof({behavior.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));""");
                }
                else
                {
                    writer.WriteLine($"""api.RegisterCollectibleBehaviorClass("{info.ModID}:{behavior.Name}", typeof({behavior.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));""");
                }
            }

            foreach(var blockEntity in info.Compilation.GetAllConcrete("Vintagestory.API.Common.BlockEntity"))
            {
                writer.WriteLine($"""api.RegisterBlockEntityClass("{info.ModID}:{blockEntity.Name}", typeof({blockEntity.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));""");
            }

            foreach(var blockEntity in info.Compilation.GetAllConcrete("Vintagestory.API.Common.BlockEntityBehavior"))
            {
                writer.WriteLine($"""api.RegisterBlockEntityBehaviorClass("{info.ModID}:{blockEntity.Name}", typeof({blockEntity.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));""");
            }

            //InsanityLib
            foreach(var blockEntity in info.Compilation.GetAllConcreteImplementations("InsanityLib.Extended.Transitions.ITransitionHandler"))
            {
                writer.WriteLine($"""InsanityLib.Extended.Transitions.CustomTransition.RegisterHandler<{blockEntity.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}>(new("{info.ModID}", "{blockEntity.Name}"));""");
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