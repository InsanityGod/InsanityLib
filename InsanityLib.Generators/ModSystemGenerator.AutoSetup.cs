using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Extensions;
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
        /// Automatically runs the setup logic (<see cref="EnsureServiceContainerPresence(ICoreAPI)" />  <see cref="AutoPatch(ICoreAPI)" />, <see cref="AutoRegistry(ICoreAPI)" />)<br/>
        /// This should be called during <see cref="ModSystem.StartPre(ICoreAPI)" />
        /// </summary>
        [MemberNotNull(nameof(ServiceContainer))]
        """);
        using (new BlockContext("protected void AutoSetup(ICoreAPI api)").Use(writer))
        {
            writer.WriteLine("_api = api;");
            writer.WriteLine("EnsureServiceContainerPresence(api);");
            writer.WriteLine("AutoPatch(api);");
            writer.WriteLine("AutoRegistry(api);");

            if (HasNetworkMessages)
            {
                writer.WriteLine("AutoNetwork(api);");
            }

            if (HasModConfigs)
            {
                writer.WriteLine(info.HasInsanityLibDependency ? "LoadAutoConfigs(api);" : "LoadConfigs(api);");
            }
        }
    }

}