using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;
using YamlDotNet.Core;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class BoolArgumentParserProvider : IArgumentParserProvider<bool>
{
    private BoolArgumentParserProvider() { }
    
    public static readonly BoolArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => new BoolArgParser(parameterInfo.GetHumanReadableName(), "on", true);
}
