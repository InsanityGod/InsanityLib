using InsanityLib.Auto.Command.Argument.Providers.Special;
using System;
using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers;
public interface IArgumentParserProvider<out T> : ICommandArgumentProvider<T>
{
    //TODO maybe look into supporting non mandatetory parameters?
    object? ICommandArgumentProvider.Provide(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, TextCommandCallingArgs currentArgs, ref int consumedParsers)
    {
        var parser = currentArgs.Parsers[consumedParsers++];

        return parser.IsMissing && !parser.IsMandatoryArg
            ? DefaultArgumentProvider.Instance.Provide(serviceProvider, argumentInfo, currentArgs, ref consumedParsers)
            : parser.GetValue();
    }

    void ICommandArgumentProvider.Configure(IServiceProvider serviceProvider, IArgumentInfo argumentInfo, List<ICommandArgumentParser> argumentParsers) 
        => argumentParsers.Add(GetParser(serviceProvider, argumentInfo));

    ICommandArgumentParser GetParser(IServiceProvider serviceProvider, IArgumentInfo argumentInfo);

}
