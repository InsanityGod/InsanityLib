using InsanityLib.Auto.Command.Argument.Providers.Special;
using System;
using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers;
public interface IArgumentParserProvider<out T> : ICommandArgumentProvider<T>
{
    //TODO maybe look into supporting non mandatetory parameters?
    object? ICommandArgumentProvider.Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, TextCommandCallingArgs currentArgs, ref int consumedParsers)
    {
        var parser = currentArgs.Parsers[consumedParsers++];

        return parser.IsMissing && !parser.IsMandatoryArg
            ? DefaultArgumentProvider.Instance.Provide(serviceProvider, parameterInfo, currentArgs, ref consumedParsers)
            : parser.GetValue();
    }

    void ICommandArgumentProvider.Configure(IServiceProvider serviceProvider, ParameterInfo parameterInfo, List<ICommandArgumentParser> argumentParsers) 
        => argumentParsers.Add(GetParser(serviceProvider, parameterInfo));

    ICommandArgumentParser GetParser(IServiceProvider serviceProvider, ParameterInfo parameterInfo);

}
