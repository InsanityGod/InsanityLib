using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class IntArgumentParserProvider : IArgumentParserProvider<int>
{
    private IntArgumentParserProvider() { }
    
    public static readonly IntArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => new IntArgParser(parameterInfo.GetHumanReadableName(), default, !parameterInfo.HasDefaultValue);
}
