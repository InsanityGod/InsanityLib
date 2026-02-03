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

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => new WorldPositionArgParser(parameterInfo.GetHumanReadableName(), serviceProvider.GetService<ICoreAPI>(), !parameterInfo.HasDefaultValue);

    object? ICommandArgumentProvider.Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, TextCommandCallingArgs currentArgs, ref int consumedParsers)
    {
        var parser = currentArgs.Parsers[consumedParsers++];

        return parser.IsMissing && !parser.IsMandatoryArg
            ? DefaultArgumentProvider.Instance.Provide(serviceProvider, parameterInfo, currentArgs, ref consumedParsers)
            : ((Vec3d)parser.GetValue())?.AsBlockPos;
    }
}
