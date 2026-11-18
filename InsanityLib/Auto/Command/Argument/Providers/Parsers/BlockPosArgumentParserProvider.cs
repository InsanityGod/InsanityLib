using InsanityLib.Auto.Command.Argument.Providers.Special;
using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;
public class BlockPosArgumentParserProvider : IArgumentParserProvider<BlockPos>
{
    private BlockPosArgumentParserProvider() { }
    
    public static readonly BlockPosArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => new WorldPositionArgParser(argumentInfo.GetHumanReadableName(), serviceProvider.GetService<ICoreAPI>(), !argumentInfo.HasDefaultValue);

    object? ICommandArgumentProvider.Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, TextCommandCallingArgs currentArgs, ref int consumedParsers)
    {
        var parser = currentArgs.Parsers[consumedParsers++];

        return parser.IsMissing && !parser.IsMandatoryArg
            ? DefaultArgumentProvider.Instance.Provide(serviceProvider, argumentInfo, currentArgs, ref consumedParsers)
            : ((Vec3d)parser.GetValue())?.AsBlockPos;
    }
}
