using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class BoolArgumentParserProvider : IArgumentParserProvider<bool>
{
    private BoolArgumentParserProvider() { }
    
    public static readonly BoolArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => new BoolArgParser(argumentInfo.GetHumanReadableName(), "on", !argumentInfo.HasDefaultValue);
}
