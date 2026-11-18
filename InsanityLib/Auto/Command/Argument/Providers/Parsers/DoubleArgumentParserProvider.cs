using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class DoubleArgumentParserProvider : IArgumentParserProvider<double>
{
    private DoubleArgumentParserProvider() { }
    
    public static readonly DoubleArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => new DoubleArgParser(argumentInfo.GetHumanReadableName(), default, !argumentInfo.HasDefaultValue);
}
