using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Extensions;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators;

public sealed partial class ModSystemGenerator
{
    private string[] ModPatchCategories;
    private bool hasPatches;

    private string[] GetOrFindModPatchCategories(GeneratorContext info) => ModPatchCategories ??= [.. 
        info.Compilation.GetSymbolsWithAttribute("HarmonyLib.HarmonyPatchCategory")
        .Select(result => $"\"{result.Attribute.ConstructorArguments[0].Value.ToString().Replace("\"", "\"\"")}\"")
        .Where(category => !category.Contains(':') || category.StartsWith("mod:", StringComparison.OrdinalIgnoreCase))
        .Distinct()
    ];

    public void GenerateStaticPatchCategoryList(IndentedTextWriter writer, GeneratorContext info)
    {
        var categories = GetOrFindModPatchCategories(info);
        if(categories.Length == 0) return;
        writer.WriteLine($"private static readonly string[] ModPatchCategories = [{string.Join(", ", categories)}];");
        writer.WriteLine();
    }

    public void GenerateAutoPatcherMethod(IndentedTextWriter writer, GeneratorContext info)
    {
        hasPatches = info.Compilation.GetSymbolsWithAttribute("HarmonyLib.HarmonyPatch").Any();
        if(!hasPatches) return;
        writer.WriteMultiLine("""
        /// <summary>
        /// Allows for defining custom patch calls that are automatically run when <see cref="AutoPatch(ICoreAPI)" /> runs.
        /// </summary>
        """);
        writer.WriteLine("partial void ManualPatches(Harmony harmony, ICoreAPI api);");
        writer.WriteLine();

        writer.WriteMultiLine("""
        /// <summary>
        /// Allows for defining custom patch calls that are automatically run when <see cref="AutoPatch(ICoreAPI)" /> runs.
        /// </summary>
        """);
        using (new BlockContext("protected bool TryPatchCategory(Harmony harmony, string category, ILogger logger)").Use(writer))
        {
            using (TryContext.Catch(CatchContext.Log("Exception", "logger.Error", "Failed compatibility patch for '{0}': {1}", "category, exception")).Use(writer))
            {
                writer.WriteLine("harmony.PatchCategory(category);");
                writer.WriteLine("return true;");
            }
            writer.WriteLine("return false;");
        }

        writer.WriteMultiLine("""
        /// <summary>
        /// Automatically patches everything uncategorized and all categories that match a modID.<br/>
        /// Implement <see cref="ManualPatches(Harmony, ICoreAPI)"/> to manually run additional patches
        /// </summary>
        """);
        using (new BlockContext("protected void AutoPatch(ICoreAPI api)").Use(writer))
        {
            writer.WriteLine($"""if (Harmony.HasAnyPatches("{info.ModID}")) return;""");
            writer.WriteLine();
            writer.WriteLine($"""var harmony = new Harmony("{info.ModID}");""");

            writer.WriteLine("harmony.PatchAllUncategorized();");

            var categories = GetOrFindModPatchCategories(info);
            if(categories.Length > 0)
            {
                using (new ForeachContext("var category", "ModPatchCategories").Use(writer))
                using (new IfContext("api.ModLoader.IsModEnabled(category)", false).Use(writer))
                {
                    writer.WriteLine("TryPatchCategory(harmony, category, Mod.Logger);");
                }
            }

            writer.WriteLine("ManualPatches(harmony, api);");
        }
    }

}