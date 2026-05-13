using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Extensions;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators;

public sealed partial class ModSystemGenerator
{
    private (ISymbol Symbol, AttributeData Attribute)[] configlist;
    private bool HasModConfigs => configlist is not null && configlist.Length > 0;

    public void GenerateAutoModConfigMethods(IndentedTextWriter writer, GeneratorContext info)
    {
        configlist = [.. info.Compilation.GetSymbolsWithAttribute("InsanityLib.Generators.Attributes.AutoConfigAttribute")];
        if(!HasModConfigs) return;

        writer.WriteMultiLine("""
        /// <summary>
        /// Registers all configs as AutoConfigs in InsanityLib
        /// </summary>
        """);

        using (new BlockContext("protected void LoadAutoConfigs(ICoreAPI api)").Use(writer))
        {
            foreach((var symbol, var attr) in configlist)
            {
                writer.Write("AutoConfig.GetOrRegister<");
                writer.Write(symbol.GetPrimaryType().ToDisplayString(SymbolExtensions.QualifiedEnoughFormat));
                writer.Write(">(api, Mod.Logger, ");
                writer.WriteLiteral(attr.ConstructorArguments[0].Value);
                writer.Write(", ");
                writer.WriteLiteral(attr.NamedArguments.GetArgument("ServerSync", false));
                writer.Write(", true, config => ");
                writer.Write(symbol.GetStaticMemberPath());
                writer.WriteLine(" = config);");
            }
        }

        if (info.HasInsanityLibDependency) return;

        writer.WriteMultiLine("""
        private T RegisterOrCollectConfigFile<T>(ICoreAPI api, string path, T result) where T : class, new() => AutoConfigLib.AutoConfigLibModSystem.RegisterOrCollectConfigFile<T>(api, path, result);
        """);
        writer.WriteLine();

        using (new BlockContext("private T LoadConfig<T>(ICoreAPI api, string path, bool serverSynced) where T : class, new()").Use(writer))
        {
            writer.WriteLine("T result;");
            using(new TryContext().Catch(new CatchContext("Exception")
            {
                Content = writer =>
                {
                    writer.WriteLine("""Mod.Logger.Error("Failed to load config '{0}' of type '{1}', using default values: {2}", path, typeof(T), exception);""");
                    writer.WriteLine("""result = new();""");
                }
            }).Use(writer))
            {
                using(new IfContext("serverSynced && api.Side == EnumAppSide.Client").Use(writer))
                {
                    writer.Write("""var configBytes = api.World.Config.GetTreeAttribute("configs").GetBytes(path) """);
                    writer.WriteLine("""?? throw new InvalidOperationException("Server config was not received");""");

                    writer.WriteLine("""return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(configBytes));""");
                }
                
                writer.WriteLine("""result = api.LoadModConfig<T>(path) ?? new();""");
                
                using(new IfContext("api.Side == EnumAppSide.Server && serverSynced").Use(writer))
                {
                    writer.WriteLine("""api.World.Config.GetOrAddTreeAttribute("configs").SetBytes(path, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result, Formatting.None)));""");
                }

                writer.WriteLine("api.StoreModConfig(result, path);");
            }

            writer.WriteLine();
            writer.WriteLine("""if(api.ModLoader.IsModEnabled("autoconfiglib")) return RegisterOrCollectConfigFile<T>(api, path, result);""");
            writer.WriteLine("return result;");
        }

        writer.WriteMultiLine("""
        /// <summary>
        /// Handles the loading of all configs and will use AutoConfigs if InsanityLib is present
        /// </summary>
        """);
        using (new BlockContext("protected void LoadConfigs(ICoreAPI api)").Use(writer))
        {
            using(new IfContext("""api.ModLoader.IsModEnabled("insanitylib")""").Use(writer))
            {
                writer.WriteLine("LoadAutoConfigs(api);");
                writer.WriteLine("return;");
            }

            foreach((var symbol, var attr) in configlist)
            {
                writer.Write(symbol.GetStaticMemberPath());
                writer.Write(" ??= LoadConfig<");
                writer.Write(symbol.GetPrimaryType().ToDisplayString(SymbolExtensions.QualifiedEnoughFormat));
                writer.Write(">(api, ");
                writer.WriteLiteral(attr.ConstructorArguments[0].Value);
                writer.Write(", ");
                writer.WriteLiteral(attr.NamedArguments.GetArgument("ServerSync", false));
                writer.WriteLine(");");
            }
        }
    }

}