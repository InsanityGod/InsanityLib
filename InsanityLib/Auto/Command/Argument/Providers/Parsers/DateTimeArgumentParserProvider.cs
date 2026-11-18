using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class DateTimeArgumentParserProvider : IArgumentParserProvider<DateTime>
{
    private DateTimeArgumentParserProvider() { }
    
    public static readonly DateTimeArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, IArgumentInfo argumentInfo) => new DatetimeArgParser(argumentInfo.GetHumanReadableName(), !argumentInfo.HasDefaultValue);
}
