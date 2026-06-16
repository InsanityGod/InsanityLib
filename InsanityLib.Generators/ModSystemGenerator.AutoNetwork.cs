using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Extensions;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Dynamic;

namespace InsanityLib.Generators;

public sealed partial class ModSystemGenerator
{
    private Dictionary<string, List<MessageHandlerInfo>> autoNetworkMessages;

    private bool HasNetworkMessages => autoNetworkMessages is not null && autoNetworkMessages.Count > 0;

    public void GenerateAutoNetworkMethod(IndentedTextWriter writer, GeneratorContext info)
    {
        autoNetworkMessages = FindAutoNetworkMessages(info);
        if(!HasNetworkMessages) return;

        writer.WriteMultiLine("""
        /// <summary>
        /// Automatically sets up the netowrk channels and handler methods
        /// </summary>
        """);
        using (new BlockContext("protected void AutoNetwork(ICoreAPI api)").Use(writer))
        {
            using(new IfContext("api is ICoreClientAPI clientApi").Use(writer))
            {
                foreach(var item in autoNetworkMessages)
                {
                    writer.Write($"clientApi.Network.RegisterChannel(\"{item.Key}\")");
                    writer.Indent++;
                    foreach(var messageHandlerInfo in item.Value)
                    {
                        writer.WriteLine();
                        writer.Write($".RegisterMessageType<{messageHandlerInfo.MessageTypeStr}>()");

                        if (messageHandlerInfo.ClientHandlerMethodStr is not null)
                        {
                            writer.Write($".SetMessageHandler<{messageHandlerInfo.MessageTypeStr}>({messageHandlerInfo.ClientHandlerMethodStr})");
                        }
                    }
                    writer.WriteLine(";");
                    writer.Indent--;
                }
            }

            using(new IfContext("api is ICoreServerAPI serverApi").Use(writer))
            {
                foreach(var item in autoNetworkMessages)
                {
                    writer.Write($"serverApi.Network.RegisterChannel(\"{item.Key}\")");
                    writer.Indent++;
                    foreach(var messageHandlerInfo in item.Value)
                    {
                        writer.WriteLine();
                        writer.Write($".RegisterMessageType<{messageHandlerInfo.MessageTypeStr}>()");

                        if (messageHandlerInfo.ServerHandlerMethodStr is not null)
                        {
                            writer.Write($".SetMessageHandler<{messageHandlerInfo.MessageTypeStr}>({messageHandlerInfo.ServerHandlerMethodStr})");
                        }
                    }
                    writer.WriteLine(";");
                    writer.Indent--;
                }
            }
        }
    }

    private static Dictionary<string, List<MessageHandlerInfo>> FindAutoNetworkMessages(GeneratorContext info)
    {
        var networkGroups = info.Compilation.GetSymbolsWithAttribute("InsanityLib.Generators.Attributes.AutoNetworkMessageAttribute")
            .GroupBy(x => x.Attribute.ConstructorArguments[0].Value as string ?? info.ModID);
        
        var results = new Dictionary<string, List<MessageHandlerInfo>>();

        foreach(var networkGroup in networkGroups)
        {
            var messageHandlerInfos = new List<MessageHandlerInfo>();
            results[networkGroup.Key] = messageHandlerInfos;

            var lookup = new Dictionary<string, (IMethodSymbol serverMethod, IMethodSymbol clientMethod)>();

            foreach (var method in networkGroup.Select(item => item.Symbol).OfType<IMethodSymbol>())
            {
                IMethodSymbol clientMethod = null;
                IMethodSymbol serverMethod = null;

                if(IsValidMethodForSide(info, method, server: true))
                {
                    var str = method.Parameters[1].Type.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat);

                    if(lookup.TryGetValue(str, out var existingMethods))
                    {
                        (serverMethod, clientMethod) = existingMethods;

                        if(serverMethod is not null)
                        {
                            info.Context.ReportDiagnostic(Diagnostic.Create(
                                Diagnostics.DuplicateMatch,
                                GetAutoNetworkAttributeLocation(method), additionalLocations: [GetAutoNetworkAttributeLocation(serverMethod)],
                                $"{method.ContainingType?.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}.{serverMethod.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}"
                            ));
                            continue;
                        }
                    }
                    serverMethod = method;

                    lookup[str] = (serverMethod, clientMethod);
                }
                else if(IsValidMethodForSide(info, method, server: false))
                {
                    var str = method.Parameters[0].Type.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat);

                    if(lookup.TryGetValue(str, out var existingMethods))
                    {
                        (serverMethod, clientMethod) = existingMethods;

                        if(clientMethod is not null)
                        {
                            info.Context.ReportDiagnostic(Diagnostic.Create(
                                Diagnostics.DuplicateMatch,
                                GetAutoNetworkAttributeLocation(method), additionalLocations: [GetAutoNetworkAttributeLocation(clientMethod)],
                                $"{method.ContainingType?.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}.{clientMethod.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}"
                            ));
                            continue;
                        }
                    }
                    clientMethod = method;

                    lookup[str] = (serverMethod, clientMethod);
                }
            }

            foreach(var item in lookup)
            {
                messageHandlerInfos.Add(new MessageHandlerInfo
                {
                    MessageTypeStr = item.Key,
                    ServerHandlerMethodStr = GetHandlerString(info, item.Value.serverMethod),
                    ClientHandlerMethodStr = GetHandlerString(info, item.Value.clientMethod)
                });
            }
        }
        return results;
    }

    private static Location GetAutoNetworkAttributeLocation(IMethodSymbol method) => method.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "InsanityLib.Generators.Attributes.AutoNetworkMessageAttribute")?.ApplicationSyntaxReference.GetSyntax().GetLocation() ?? method.Locations[0];

    private static string GetHandlerString(GeneratorContext info, IMethodSymbol method)
    {
        if (method is null || method.ContainingType is null || info.ContainingType is null) return null;
        if (info.ContainingType.Equals(method.ContainingType, SymbolEqualityComparer.Default)) return method.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat);
        if (method.IsStatic) return $"{method.ContainingType.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}.{method.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}";

        var prefix = info.ContainingType.DerivesFrom(info.Compilation.GetTypeByMetadataName("Vintagestory.API.Common.ModSystem"))  ? "api.ModLoader.GetModSystem" : "ServiceContainer.GetService";

        return $"{prefix}<{method.ContainingType.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}>().{method.ToDisplayString(SymbolExtensions.QualifiedEnoughFormat)}";
    }

    private static bool IsValidMethodForSide(GeneratorContext info, IMethodSymbol method, bool server)
    {
        if (server)
        {
            if(method.Parameters.Length != 2) return false;
            var iServerPlayerType = info.Compilation.GetTypeByMetadataName("Vintagestory.API.Server.IServerPlayer");

            if (!method.Parameters[0].Type.Equals(iServerPlayerType, SymbolEqualityComparer.Default)) return false;
        }
        else if(method.Parameters.Length != 1) return false;

        return true;
    }
}

public class MessageHandlerInfo
{
    public string MessageTypeStr { get; set; }

    public string ServerHandlerMethodStr { get; set; }

    public string ClientHandlerMethodStr { get; set; }
}
