using InsanityLib.Auto.Command.Argument.Providers.Special;
using InsanityLib.Extended.Enums;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;
public class EnumArgumentParserProvider : IArgumentParserProvider<Enum>
{
    private EnumArgumentParserProvider() { }
    
    public static readonly EnumArgumentParserProvider Instance = new();

    public bool CanProvide(ParameterInfo paramInfo, CommandParameterAttribute? attr) => paramInfo.ParameterType.IsEnum;

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, ParameterInfo parameterInfo)
    {
        //TODO maybe just make a straight EnumArgParser that takes in the Type instead of doing this every time

        var mapping = new EnumNameValueMapping(parameterInfo.ParameterType);
        
        return new WordRangeArgParser(
            parameterInfo.GetHumanReadableName(),
            !parameterInfo.HasDefaultValue,
            
            mapping.Names
        );
    }

    object? ICommandArgumentProvider.Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, TextCommandCallingArgs currentArgs, ref int consumedParsers)
    {
        var parser = currentArgs.Parsers[consumedParsers++];

        var value = parser.IsMissing && !parser.IsMandatoryArg
            ? DefaultArgumentProvider.Instance.Provide(serviceProvider, parameterInfo, currentArgs, ref consumedParsers)
            : parser.GetValue();
        if(value is not string strValue) return value;

        return EnumExtensionUtil.TryParse(parameterInfo.ParameterType, strValue);
    }
}
