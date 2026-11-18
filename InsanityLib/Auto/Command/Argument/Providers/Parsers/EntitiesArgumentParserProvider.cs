using InsanityLib.Extensions;
using System;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class EntitiesArgumentParserProvider : IArgumentParserProvider<Entity[]>
{
    private EntitiesArgumentParserProvider() { }
    
    bool ICommandArgumentProvider.CanProvide(IArgumentInfo argumentInfo, CommandParameterAttribute? attr) => typeof(Entity[]).IsAssignableFrom(argumentInfo.Type) || typeof(Entity).IsAssignableFrom(argumentInfo.Type);

    public static readonly EntitiesArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => new EntitiesArgParser(argumentInfo.GetHumanReadableName(), serviceProvider.GetService<ICoreAPI>(), false);

    object? ICommandArgumentProvider.Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, TextCommandCallingArgs currentArgs, ref int consumedParsers)
    {
        var parser = currentArgs.Parsers[consumedParsers++];

        if(parser.IsMissing && argumentInfo.HasDefaultValue) return argumentInfo.DefaultValue;

        Entity[]? entities = (Entity[]?)parser.GetValue();
        if(entities is null) return null;

        Type target = argumentInfo.Type.GetElementType() ?? argumentInfo.Type;

        var validEntries = entities.Where(target.IsInstanceOfType).ToArray();
        var typedValidEntries = Array.CreateInstance(target, validEntries.Length);
        validEntries.CopyTo(typedValidEntries, 0);

        return typedValidEntries;
    }
}
