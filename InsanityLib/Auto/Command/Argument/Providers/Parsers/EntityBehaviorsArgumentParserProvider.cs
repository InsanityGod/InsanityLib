using InsanityLib.Extensions;
using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class EntityBehaviorsArgumentParserProvider : IArgumentParserProvider<EntityBehavior[]>
{
    private EntityBehaviorsArgumentParserProvider() { }
    
    bool ICommandArgumentProvider.CanProvide(IArgumentInfo argumentInfo, CommandParameterAttribute? attr) => typeof(EntityBehavior[]).IsAssignableFrom(argumentInfo.Type) || typeof(EntityBehavior).IsAssignableFrom(argumentInfo.Type);

    public static readonly EntityBehaviorsArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => new EntitiesArgParser(argumentInfo.GetHumanReadableName(), serviceProvider.GetService<ICoreAPI>(), false);

    object? ICommandArgumentProvider.Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, TextCommandCallingArgs currentArgs, ref int consumedParsers)
    {
        var parser = currentArgs.Parsers[consumedParsers++];
        
        if(parser.IsMissing && argumentInfo.HasDefaultValue) return argumentInfo.DefaultValue;

        Entity[]? entities = (Entity[]?)parser.GetValue();
        if(entities is null) return null;

        Type target = argumentInfo.Type.GetElementType() ?? argumentInfo.Type;

        var validEntries = entities.Select(entity => entity.SidedProperties.Behaviors)
            .Where(behs => behs is not null)
            .SelectMany(behs => behs.Where(beh => target.IsInstanceOfType(beh)))
            .ToArray();

        var typedValidEntries = Array.CreateInstance(target, validEntries.Length);
        validEntries.CopyTo(typedValidEntries, 0);

        return typedValidEntries;
    }
}
