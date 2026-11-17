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

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => new PlayersArgParser(argumentInfo.GetHumanReadableName(), serviceProvider.GetService<ICoreAPI>(), !argumentInfo.HasDefaultValue);
}
