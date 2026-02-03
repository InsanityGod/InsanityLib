using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Extensions;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators;

public sealed partial class ModSystemGenerator
{
    public void GenerateServiceLogic(IndentedTextWriter writer, GeneratorContext info)
    {
        writer.WriteMultiLine("""
        /// <summary>
        /// Access to the service container for this side
        /// </summary>
        """);

        writer.WriteLine("public IServiceContainer? ServiceContainer { get; private set; }");
        writer.WriteLine();

        writer.WriteMultiLine("""
        /// <summary>
        /// Ensure that <see cref="ServiceContainer" /> is loaded
        /// </summary>
        [MemberNotNull(nameof(ServiceContainer))]
        """);
        using (new BlockContext("protected void EnsureServiceContainerPresence(ICoreAPI api)").Use(writer))
        {
            writer.WriteLine("if(ServiceContainer is not null) return;");
            using(new IfContext("""api.ObjectCache.TryGetValue("insanitylib:ServiceContainer", out object? serviceContainer)""").Use(writer))
            {
                writer.WriteLine("ServiceContainer = (ServiceContainer)serviceContainer!;");
                writer.WriteLine("return;");
            }
            writer.Write("""api.ObjectCache["insanitylib:ServiceContainer"] = ServiceContainer = new ServiceContainer(""");
            if (info.HasInsanityLibDependency)
            {
                writer.Write("InsanityLib.InsanityLibModSystem.GlobalServiceContainer");
            }
            else writer.Write("""api.ModLoader.IsModEnabled("insanitylib") ? InsanityLib.InsanityLibModSystem.GlobalServiceContainer : null""");
            writer.WriteLine(");");

            writer.WriteLine();
            writer.WriteLine("//Register commen services");
            writer.WriteLine("ServiceContainer.AddService(typeof(ICoreAPI), api);");
            writer.WriteLine("ServiceContainer.AddService(typeof(IWorldAccessor), api.World);");
            writer.WriteLine("ServiceContainer.AddService(typeof(ILogger), api.Logger);");
        }
    }

}