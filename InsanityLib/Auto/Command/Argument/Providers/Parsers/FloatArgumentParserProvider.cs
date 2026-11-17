using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class FloatArgumentParserProvider : IArgumentParserProvider<float>
{
    private FloatArgumentParserProvider() { }
    
    public static readonly FloatArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => new FloatArgParser(argumentInfo.GetHumanReadableName(), !argumentInfo.HasDefaultValue);
}
