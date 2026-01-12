using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class EntityPropertiesArgumentParserProvider : IArgumentParserProvider<EntityProperties>
{
    private EntityPropertiesArgumentParserProvider() { }
    
    public static readonly EntityPropertiesArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, ParameterInfo parameterInfo) => new EntityTypeArgParser(parameterInfo.GetHumanReadableName(), serviceProvider.GetService<ICoreAPI>(), !parameterInfo.HasDefaultValue);
}
