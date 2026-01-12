using InsanityLib.Auto.Cleanup;
using InsanityLib.Auto.Command.Argument.Providers.Contextual;
using InsanityLib.Auto.Command.Argument.Providers.Parsers;
using InsanityLib.Auto.Command.Argument.Providers.Special;
using InsanityLib.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command.Argument.Providers;

public interface ICommandArgumentProvider<out T> : ICommandArgumentProvider
{
    //TODO see if we can support convertable types
    bool ICommandArgumentProvider.CanProvide(ParameterInfo paramInfo, CommandParameterAttribute? attr) => paramInfo.ParameterType.IsAssignableFrom(typeof(T));
}

public interface ICommandArgumentProvider
{
    bool CanProvide(ParameterInfo paramInfo, CommandParameterAttribute? attr);

    void Configure(IServiceProvider serviceProvider, ParameterInfo parameterInfo, List<ICommandArgumentParser> argumentParsers)
    {
        //Optional to implement
    }

    object? Provide(IServiceProvider serviceProvider, ParameterInfo parameterInfo, TextCommandCallingArgs currentArgs, ref int consumedParsers);

    /// <summary>
    /// Find the argument provider for a given parameter.
    /// </summary>
    public static ICommandArgumentProvider? Find(ParameterInfo paramInfo)
    {
        var attr = paramInfo.GetCustomAttribute<CommandParameterAttribute>();
            
        ICommandArgumentProvider? argumentProvider = null;
        switch (attr?.Provider)
        {
            case EParamProvider.ServiceProvider:
                if (!ServiceArgumentProvider.Instance.CanProvide(paramInfo, attr)) break;

                argumentProvider = ServiceArgumentProvider.Instance;
                break;

            case EParamProvider.ArgumentParser:
                argumentProvider = ArgumentParserProviders.FirstOrDefault(provider => provider.CanProvide(paramInfo, attr));
                break;


            case EParamProvider.ContextualProvider:
                argumentProvider = ContextualArgumentProviders.FirstOrDefault(provider => provider.CanProvide(paramInfo, attr));
                break;

            case EParamProvider.DefaultValue:
                argumentProvider = DefaultArgumentProvider.Instance;
                break;

            case null:
                argumentProvider = ArgumentParserProviders.FirstOrDefault(provider => provider.CanProvide(paramInfo, attr));
                if (argumentProvider is not null) break;

                argumentProvider = ContextualArgumentProviders.FirstOrDefault(provider => provider.CanProvide(paramInfo, attr));
                if (argumentProvider is not null) break;

                var defaultAttr = paramInfo.GetCustomAttribute<DefaultValueAttribute>();
                if (defaultAttr is AutoDefaultValueAttribute) goto case EParamProvider.DefaultValue;
                else if(defaultAttr is not null)
                {
                    if(defaultAttr.Value is null && ServiceArgumentProvider.Instance.CanProvide(paramInfo, attr))
                    {
                        goto case EParamProvider.ServiceProvider;
                    }
                    goto case EParamProvider.DefaultValue;
                }

                goto case EParamProvider.ServiceProvider;
        }

        return argumentProvider;
    }

    /// <summary>
    /// Find the argument providers for all parameters of a given method.
    /// </summary>
    /// <exception cref="InvalidOperationException" />
    public static ICommandArgumentProvider[] Find(MethodBase method)
    {
        var parameters = method.GetParameters();
        ICommandArgumentProvider[] providers = new ICommandArgumentProvider[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var paramInfo = parameters[i];
            
            providers[i] = Find(paramInfo) ?? throw new InvalidAttributeUsageException($"No valid argument provider exists for parameter '{paramInfo.ParameterType.FullName} {paramInfo.Name}'");
        }

        return providers;
    }

    public static ICommandArgumentProvider[] ArgumentParserProviders { get; set; } = [
        BoolArgumentParserProvider.Instance,
        ColorArgumentParserProvider.Instance,
        DateTimeArgumentParserProvider.Instance,
        BlockPosArgumentParserProvider.Instance,
        Vec3DArgumentParserProvider.Instance,
        Vec2DArgumentParserProvider.Instance,
        EntitiesArgumentParserProvider.Instance,
        EntityPropertiesArgumentParserProvider.Instance,
        OnlinePlayerArgumentParserProvider.Instance,
        PlayerRoleArgumentParserProvider.Instance,
        PlayerUidNamesArgumentParserProvider.Instance,
        LongArgumentParserProvider.Instance,
        IntArgumentParserProvider.Instance,
        DoubleArgumentParserProvider.Instance,
        FloatArgumentParserProvider.Instance,
        StringArgumentParserProvider.Instance,
        EnumArgumentParserProvider.Instance,
    ];

    public static ICommandArgumentProvider[] ContextualArgumentProviders { get; set; } = [

        ContextualPlayerProvider.Instance,
        ContextualEntityProvider.Instance,
        ContextualEntityBehaviorProvider.Instance,
        ContextualBlockPosProvider.Instance,
        ContextualItemSlotProvider.Instance,
        ContextualItemStackProvider.Instance,
        ContextualBlockSelectionProvider.Instance,
        ContextualCollectibleProvider.Instance,
        ContextualCollectibleBehaviorProvider.Instance,
        ContextualBlockEntityProvider.Instance,
        ContextualBlockEntityBehaviorProvider.Instance,
    ];
}
