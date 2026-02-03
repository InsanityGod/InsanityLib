using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class LongArgumentParserProvider : IArgumentParserProvider<long>
{
    private LongArgumentParserProvider() { }
    
    public static readonly LongArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => new LongArgParser(parameterInfo.GetHumanReadableName(), default, !parameterInfo.HasDefaultValue);
}
