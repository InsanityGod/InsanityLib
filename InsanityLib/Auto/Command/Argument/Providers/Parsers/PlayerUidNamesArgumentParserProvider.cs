using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;
public class PlayerUidNamesArgumentParserProvider : IArgumentParserProvider<PlayerUidName[]>
{
    private PlayerUidNamesArgumentParserProvider() { }
    
    public static readonly PlayerUidNamesArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => new PlayersArgParser(parameterInfo.GetHumanReadableName(), serviceProvider.GetService<ICoreAPI>(), !parameterInfo.HasDefaultValue);
}
