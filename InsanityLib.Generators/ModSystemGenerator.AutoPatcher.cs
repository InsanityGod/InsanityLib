using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Extensions;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators;

public sealed partial class ModSystemGenerator
{
    private string[] patchCategories;
    private bool hasPatches;

    private string[] GetOrFindPatchCategories(GeneratorContext info) => patchCategories ??= [.. 
        info.Compilation.GetSymbolsWithAttribute("HarmonyLib.HarmonyPatchCategory")
        .Select(result => $"\"{result.Attribute.ConstructorArguments[0].Value.ToString().Replace("\"", "\"\"")}\"")
    ];

    public void GenerateStaticPatchCategoryList(IndentedTextWriter writer, GeneratorContext info)
    {
        var categories = GetOrFindPatchCategories(info);
        if(categories.Length == 0) return;
        writer.WriteLine($"private static readonly string[] PatchCategories = [{string.Join(", ", categories)}];");
        writer.WriteLine();
    }

    public void GenerateAutoPatcherMethod(IndentedTextWriter writer, GeneratorContext info)
    {
        writer.WriteMultiLine("""
        /// <summary>
        /// Allows for defining custom patch calls that are automatically run when <see cref="AutoPatch(ICoreAPI)" /> runs.
        /// </summary>
        """);
        writer.WriteLine("partial void ManualPatches(Harmony harmony, ICoreAPI api);");
        writer.WriteLine();

        writer.WriteMultiLine("""
        /// <summary>
        /// Automatically patches everything uncategorized and all categories that match a modID.<br/>
        /// Implement <see cref="ManualPatches(Harmony, ICoreAPI)"/> to manually run additional patches
        /// </summary>
        """);
        using (new BlockContext("protected void AutoPatch(ICoreAPI api)").Use(writer))
        {
            hasPatches = info.Compilation.GetSymbolsWithAttribute("HarmonyLib.HarmonyPatch").Any();

            if (hasPatches)
            {
                writer.WriteLine($"""if (Harmony.HasAnyPatches("{info.ModID}")) return;""");
                writer.WriteLine();
                writer.WriteLine($"""var harmony = new Harmony("{info.ModID}");""");

                writer.WriteLine("harmony.PatchAllUncategorized();");

                var categories = GetOrFindPatchCategories(info);
                if(categories.Length > 0)
                {
                    using (new ForeachContext("category", "PatchCategories").Use(writer))
                    using (new IfContext("api.ModLoader.IsModEnabled(category)").Use(writer))
                    using (TryContext.Catch(CatchContext.Log("Exception", "Mod.Logger.Error", "Failed compatibility patch for '{0}': {1}", "category, exception")).Use(writer))
                    {
                        writer.WriteLine("harmony.PatchCategory(category);");
                    }
                }

                writer.WriteLine("ManualPatches(harmony, api);");
            }
        }
    }

}