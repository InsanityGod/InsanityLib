using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Extensions;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators;

public sealed partial class ModSystemGenerator
{
    public void GenerateAutoRegistryMethod(IndentedTextWriter writer, GeneratorContext info)
    {
        writer.WriteMultiLine("""
        /// <summary>
        /// Automatically registers implementations of the following to the class registry: <see cref="Vintagestory.API.Common.Item" />, <see cref="Vintagestory.API.Common.Block" />, <see cref="Vintagestory.API.Common.CollectibleBehavior" />, <see cref="Vintagestory.API.Common.BlockBehavior" />, <see cref="Vintagestory.API.Common.BlockEntity" />, <see cref="Vintagestory.API.Common.BlockEntityBehavior" /> and <see cref="InsanityLib.Extended.Transitions.ITransitionHandler" />.<br/>
        /// Naming Scheme: "modid:classname"
        /// </summary>
        """);

        using(new BlockContext("protected void AutoClassRegistry(ICoreAPI api)").Use(writer))
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
                writer.WriteLine($"""InsanityLib.Util.ContentFeatures.CustomTransition.RegisterHandler("{info.ModID}:{blockEntity.Name}", typeof({blockEntity.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}));""");
            }
        }
    }

}