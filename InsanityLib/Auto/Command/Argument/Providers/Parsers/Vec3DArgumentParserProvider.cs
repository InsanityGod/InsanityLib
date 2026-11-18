using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class Vec3DArgumentParserProvider : IArgumentParserProvider<Vec3i>
{
    private Vec3DArgumentParserProvider() { }
    
    public static readonly Vec3DArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => new Vec3iArgParser(argumentInfo.GetHumanReadableName(), serviceProvider.GetService<ICoreAPI>(), !argumentInfo.HasDefaultValue);
}
