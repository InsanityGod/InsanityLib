using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class DoubleArgumentParserProvider : IArgumentParserProvider<double>
{
    private DoubleArgumentParserProvider() { }
    
    public static readonly DoubleArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => new DoubleArgParser(parameterInfo.GetHumanReadableName(), default, !parameterInfo.HasDefaultValue);
}
