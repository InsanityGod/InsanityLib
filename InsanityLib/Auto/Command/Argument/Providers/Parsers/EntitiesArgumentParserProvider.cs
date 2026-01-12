using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class EntitiesArgumentParserProvider : IArgumentParserProvider<Entity[]>
{
    private EntitiesArgumentParserProvider() { }
    
    public static readonly EntitiesArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => new EntitiesArgParser(parameterInfo.GetHumanReadableName(), serviceProvider.GetService<ICoreAPI>(), !parameterInfo.HasDefaultValue);
}
