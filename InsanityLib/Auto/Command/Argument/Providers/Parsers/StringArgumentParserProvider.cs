using InsanityLib.Util;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers.Parsers;

public class StringArgumentParserProvider : IArgumentParserProvider<string>
{
    private StringArgumentParserProvider() { }
    
    public static readonly StringArgumentParserProvider Instance = new();

    public ICommandArgumentParser GetParser(IServiceProvider serviceProvider, IArgumentInfo argumentInfo)
    {
        var allowedValuesAttr = argumentInfo.GetCustomAttribute<AllowedValuesAttribute>();

        if(allowedValuesAttr is not null)
        {
            return new WordRangeArgParser(
                argumentInfo.GetHumanReadableName(),
                !argumentInfo.HasDefaultValue,
                [.. allowedValuesAttr.Values.Select(obj => obj!.ToString())]
            );
        }

        return new WordArgParser(argumentInfo.GetHumanReadableName(), !argumentInfo.HasDefaultValue);
    }
}
