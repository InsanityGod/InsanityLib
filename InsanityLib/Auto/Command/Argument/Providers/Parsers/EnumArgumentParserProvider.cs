using InsanityLib.Auto.Command.Argument.Providers.Special;
using InsanityLib.Extended.Enums;
using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;
public class EnumArgumentParserProvider : IArgumentParserProvider<Enum>
{
    private EnumArgumentParserProvider() { }
    
    public static readonly EnumArgumentParserProvider Instance = new();

    public bool CanProvide(IArgumentInfo argumentInfo, CommandParameterAttribute? attr) => argumentInfo.Type.IsEnum;

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, IArgumentInfo argumentInfo)
    {
        //TODO maybe just make a straight EnumArgParser that takes in the Type instead of doing this every time

        var mapping = new EnumNameValueMapping(argumentInfo.Type);
        
        return new WordRangeArgParser(
            argumentInfo.GetHumanReadableName(),
            !argumentInfo.HasDefaultValue,
            
            mapping.Names
        );
    }

    object? ICommandArgumentProvider.Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, TextCommandCallingArgs currentArgs, ref int consumedParsers)
    {
        var parser = currentArgs.Parsers[consumedParsers++];

        var value = parser.IsMissing && !parser.IsMandatoryArg
            ? DefaultArgumentProvider.Instance.Provide(serviceProvider, argumentInfo, currentArgs, ref consumedParsers)
            : parser.GetValue();
        if(value is not string strValue) return value;

        return ExtendedEnumExtensions.TryParse(argumentInfo.Type, strValue);
    }
}
