using InsanityLib.Auto.Command.Argument.Providers;
using InsanityLib.Constants;
using InsanityLib.Documentation;
using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command;

/// <summary>
/// Represents an auto command.<br/> 
/// If manually creating this, remember to call <see cref="GetOrRegister(ICoreAPI)"/> to actually register it.
/// </summary>
public sealed class AutoCommand(MethodBase method, string? path = null, bool requiresPlayer = false, IServiceProvider? customServiceProvider = null) : IServiceProvider
{
    /// <summary>
    /// The service provider used to resolve services for this command.
    /// </summary>
    private IServiceProvider? ServiceProvider = customServiceProvider;

    public readonly MethodBase Method = method;

    public readonly string Path = path ?? method.Name.ToLower();

    private IChatCommand? _chatCommand;

    public bool IsRegistered => _chatCommand is not null;

    //TODO maybe a warning if someone attempts to register the same instance on both sides?
    public IChatCommand GetOrRegister(ICoreAPI api) => _chatCommand ??= Register(api);

    public ICommandArgumentProvider[] Providers { get; init; } = ICommandArgumentProvider.Find(method);

    /// <summary>
    /// Permissions required to run this command
    /// </summary>
    public string? RequiredPermission { get; init; }

    private IChatCommand Register(ICoreAPI api)
    {
        ServiceProvider ??= api.GetServiceProvider();

        var command = 
            api.ChatCommands
            .GetOrCreateStub(Path)
            .HandleWith(RunCommand)
            .WithDefaultConfiguration();
            
        if(!string.IsNullOrEmpty(RequiredPermission)) command.RequiresPrivilege(RequiredPermission);

        var parameters = Method.GetParameters();
        List<ICommandArgumentParser> argumentParsers = new(parameters.Length);
        for (int i = 0; i < parameters.Length; i++)
        {
            Providers[i].Configure(this, parameters[i], argumentParsers);
        }
        if(argumentParsers.Count > 0) command.WithArgs([.. argumentParsers]);

        if (requiresPlayer) command.RequiresPlayer();

        var doc = Method.GetDocumentationContext();

        var descriptionStr = doc.GetDescription();
        if(!string.IsNullOrEmpty(descriptionStr)) command.WithDescription(descriptionStr);

        var exmaplesStrings = doc.GetExamples();
        if (exmaplesStrings.Length > 0) command.WithExamples(exmaplesStrings);

        var returnStr = doc.GetReturn();
        if(!string.IsNullOrEmpty(returnStr)) command.WithAdditionalInformation($"Returns: {returnStr}");

        return command;
    }

    public TextCommandCallingArgs? CurrentArgs { get; private set; }

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(AutoCommand) || serviceType == typeof(IServiceProvider)) return this;
        if (serviceType == typeof(IChatCommand)) return _chatCommand;
        if (serviceType == typeof(TextCommandCallingArgs)) return CurrentArgs;
        if (serviceType == typeof(Caller)) return CurrentArgs?.Caller;
        if (typeof(IPlayer) == serviceType) return CurrentArgs?.Caller.Player;

        return ServiceProvider?.GetService(serviceType);
    }

    private object?[] GetAndValidateArguments()
    {
        var parameterInfos = Method.GetParameters();
        var parameters = new object?[parameterInfos.Length];
        
        int consumedParsers = 0;
        for (int i = 0; i < parameterInfos.Length; i++)
        {
            var parameterInfo = parameterInfos[i];
            var parameter =  Providers[i].Provide(this, parameterInfo, CurrentArgs!, ref consumedParsers);

            parameterInfo.Validate(this, parameter);

            parameters[i] = parameter;
        }

        return parameters;
    }

    public TextCommandResult RunCommand(TextCommandCallingArgs args)
    {
        CurrentArgs = args;
        try
        {
            object? instance = Method.IsStatic || Method.DeclaringType is null ? null : GetService(Method.DeclaringType);
            var result = Method.Invoke(instance, GetAndValidateArguments());
            
            if (result is TextCommandResult textCommandResult) return textCommandResult;
            return TextCommandResult.Success(result is null ? string.Empty : result.ToString(), result);
        }
        catch(ValidationException ex)
        {
            return TextCommandResult.Error(ex.Message);
        }
        catch (Exception ex)
        {
            this.GetService<ILogger>()?.Error(Logging.ExternalExecutionFailed, Method.FindModName(), nameof(RunCommand), Method.GetDebugDisplayName(), ex);
            return TextCommandResult.Error(ex.InnerException?.Message);
        }
        finally
        {
            CurrentArgs = null;
        }
    }
}