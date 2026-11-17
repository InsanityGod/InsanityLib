using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class Vec2DArgumentParserProvider : IArgumentParserProvider<Vec2i>
{
    private Vec2DArgumentParserProvider() { }
    
    public static readonly Vec2DArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => new WorldPosition2DArgParser(argumentInfo.GetHumanReadableName(), serviceProvider.GetService<ICoreAPI>(), !argumentInfo.HasDefaultValue);
}
