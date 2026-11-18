using InsanityLib.Util;
using System;
using System.Drawing;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class ColorArgumentParserProvider : IArgumentParserProvider<Color>
{
    private ColorArgumentParserProvider() { }
    
    public static readonly ColorArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => new ColorArgParser(argumentInfo.GetHumanReadableName(), !argumentInfo.HasDefaultValue);
}
