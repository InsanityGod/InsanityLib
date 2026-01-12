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

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => new ColorArgParser(parameterInfo.GetHumanReadableName(), !parameterInfo.HasDefaultValue);
}
