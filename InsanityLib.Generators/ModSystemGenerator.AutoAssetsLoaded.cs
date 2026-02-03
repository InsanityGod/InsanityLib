using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Extensions;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators;

public sealed partial class ModSystemGenerator
{

    public void GenerateAutoAssetsLoadedMethod(IndentedTextWriter writer, GeneratorContext info)
    {
        writer.WriteMultiLine("""
        /// <summary>
        /// Automatically handles stuff that should happen when the assets are loaded
        /// </summary>
        """);
        using (new BlockContext("protected void AutoAssetsLoaded(ICoreAPI api)").Use(writer))
        {
            if(assetCategoryAttributesWithLoadMethod.Length > 0)
            {
                writer.WriteLine("LoadAssetCategories(api);");
            }
        }
    }

}