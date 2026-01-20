using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Extensions;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators;

public sealed partial class ModSystemGenerator
{

    public void GenerateAutoSetupMethod(IndentedTextWriter writer, GeneratorContext info)
    {
        writer.WriteMultiLine("""
        /// <summary>
        /// Reference to the ICoreAPI (usefull for checking which side you are on)
        /// </summary>
        """);

        writer.WriteLine("private ICoreAPI? _api;");
        writer.WriteLine();

        writer.WriteMultiLine("""
        /// <summary>
        /// Automatically runs the setup logic (<see cref="EnsureServiceContainerPresence(ICoreAPI)" />  <see cref="AutoPatch(ICoreAPI)" />, <see cref="AutoClassRegistry(ICoreAPI)" />)<br/>
        /// This should be called during <see cref="ModSystem.StartPre(ICoreAPI)" />
        /// </summary>
        [MemberNotNull(nameof(ServiceContainer))]
        """);
        using (new BlockContext("protected void AutoSetup(ICoreAPI api)").Use(writer))
        {
            writer.WriteLine("_api = api;");
            writer.WriteLine("EnsureServiceContainerPresence(api);");
            writer.WriteLine("AutoPatch(api);");
            writer.WriteLine("AutoClassRegistry(api);");
        }
    }

}