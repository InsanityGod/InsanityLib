using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class PlayerRoleArgumentParserProvider : IArgumentParserProvider<IPlayerRole>
{
    private PlayerRoleArgumentParserProvider() { }
    
    public static readonly PlayerRoleArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => new PlayerRoleArgParser(parameterInfo.GetHumanReadableName(), serviceProvider.GetService<ICoreAPI>(), !parameterInfo.HasDefaultValue);
}
