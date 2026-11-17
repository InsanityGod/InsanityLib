using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

//TODO allow requesting specific Entity Types
//TODO allow selecting behaviors
public class EntitiesArgumentParserProvider : IArgumentParserProvider<Entity[]>
{
    private EntitiesArgumentParserProvider() { }
    
    bool ICommandArgumentProvider.CanProvide(IArgumentInfo argumentInfo, CommandParameterAttribute? attr) => argumentInfo.Type.IsAssignableFrom(typeof(Entity[])) || argumentInfo.Type.IsAssignableFrom(typeof(Entity));

    public static readonly EntitiesArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => new EntitiesArgParser(argumentInfo.GetHumanReadableName(), serviceProvider.GetService<ICoreAPI>(), !argumentInfo.HasDefaultValue);
}
