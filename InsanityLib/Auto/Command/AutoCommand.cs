using InsanityLib.Auto.Command.Argument;
using InsanityLib.Auto.Command.Argument.Providers;
using InsanityLib.Constants;
using InsanityLib.Documentation;
using InsanityLib.Extensions;
using InsanityLib.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Command; //TODO alias support

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
        //TODO create abstraction over ParameterInfo and include `this` as entry
        var parameters = Method.GetArgumentInfos();
        List<ICommandArgumentParser> argumentParsers = new(parameters.Length);
        for (int i = 0; i < parameters.Length; i++)
        {
            Providers[i].Configure(this, parameters[i], argumentParsers);
        }
        if(argumentParsers.Count > 0) command.WithArgs([.. argumentParsers]);

        if (requiresPlayer) command.RequiresPlayer();

        var doc = Method.GetDocumentationContext();
        if(doc is not null)
        {
            var descriptionStr = doc.GetDescription();
            if(!string.IsNullOrEmpty(descriptionStr)) command.WithDescription(descriptionStr);

            var exmaplesStrings = doc.GetExamples();
            if (exmaplesStrings.Length > 0) command.WithExamples(exmaplesStrings);

            var returnStr = doc.GetReturn();
            if(!string.IsNullOrEmpty(returnStr)) command.WithAdditionalInformation($"Returns: {returnStr}");
        }

        return command;
    }

    public TextCommandCallingArgs? CurrentArgs { get; private set; }

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(AutoCommand) || serviceType == typeof(IServiceProvider)) return this;
        if (serviceType == typeof(IChatCommand)) return _chatCommand;


        return ServiceProvider?.GetService(serviceType);
    }

    public TextCommandResult RunCommand(TextCommandCallingArgs args)
    {
        var run = new AutoCommandRun(this, args);

        CurrentArgs = args;
        try
        {
            return run.Execute();
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

public sealed class AutoCommandRun(AutoCommand command, TextCommandCallingArgs args) : IServiceProvider
{
    public AutoCommand Command { get; } = command;

    public TextCommandCallingArgs Args { get;  } = args;

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(AutoCommandRun) || serviceType == typeof(IServiceProvider)) return this;
        if (serviceType == typeof(TextCommandCallingArgs)) return Args;
        if (serviceType == typeof(Caller)) return Args.Caller;
        if (typeof(IPlayer) == serviceType) return Args.Caller.Player;
        
        return Command.GetService(serviceType);
    }

    private List<object?> FilterValidArgs(IArgumentInfo argInfo, IEnumerable enumerable, out int failed)
    {
        var valid = new List<object?>();
        failed = 0;
        
        foreach(var entry in enumerable)
        {
            try
            {
                argInfo.Validate(this, entry);
                valid.Add(entry);
            }
            catch
            {
                failed++;
            }
        }

        if(valid.Count == 0) throw new ValidationException($"argument '{argInfo.GetHumanReadableName()}' had zero matches"); 

        return valid;
    }

    private static IEnumerable<object?[]> GetArgumentCombinations(object?[] args, bool[] argIsCollection)
    {
        var current = new object?[args.Length];
    
        for (int i = 0; i < args.Length; i++)
        {
            if (argIsCollection[i]) continue;
            
            current[i] = args[i];
        }

        return Generate(0);
    
        IEnumerable<object?[]> Generate(int index)
        {
            if (index == args.Length)
            {
                yield return (object?[]) current.Clone();
                yield break;
            }
    
            if (!argIsCollection[index])
            {
                foreach (var result in Generate(index + 1)) yield return result;
                yield break;
            }
    
            foreach (var value in (IEnumerable)args[index]!)
            {
                current[index] = value;
    
                foreach (var result in Generate(index + 1)) yield return result;
            }
        }
    }

    public TextCommandResult Execute()
    {
        var method = Command.Method;
        var argumentInfos = method.GetArgumentInfos();

        var validatedArgs = new object?[argumentInfos.Length];
        var argIsCollection = new bool [argumentInfos.Length];
        var skippedCollectionItems = new int[argumentInfos.Length];

        int consumedParsers = 0;
        for (int i = 0; i < argumentInfos.Length; i++)
        {
            var argInfo = argumentInfos[i];
            var parsedArg = Command.Providers[i].Provide(this, argInfo, Args!, ref consumedParsers);

            if (!argInfo.Type.IsInstanceOfType(parsedArg))
            {
                if (typeof(IEnumerable<>).MakeGenericType(argInfo.Type).IsAssignableFrom(parsedArg?.GetType()))
                {
                    parsedArg = FilterValidArgs(argInfo, (IEnumerable)parsedArg, out skippedCollectionItems[i]);
                    argIsCollection[i] = true;
                }
                else parsedArg = parsedArg.AutoConvert(argInfo.Type);
            }
            else argInfo.Validate(this, parsedArg);

            validatedArgs[i] = parsedArg;
        }

        int combinationCount = 0;
        int failedCount = 0;
        object? lastResult = null;

        foreach(var combination in GetArgumentCombinations(validatedArgs, argIsCollection))
        {
            combinationCount++;
            try
            {
                var result = method.IsStatic ?
                    method.Invoke(null, combination) :
                    method.Invoke(combination[0], combination[1..]);

                if (result is TextCommandResult textCommandResult)
                {
                    lastResult = textCommandResult;
                    if(textCommandResult.Status == EnumCommandStatus.Error) failedCount++;
                }
                else lastResult = result;
            }
            catch (Exception ex)
            {
                failedCount++;

                this.GetService<ILogger>()?.Error(Logging.ExternalExecutionFailed, method.FindModName(), nameof(AutoCommandRun), method.GetDebugDisplayName(), ex);
                lastResult = TextCommandResult.Error(ex.InnerException?.Message);
            }
        }

        if(combinationCount == 1)
        {
            if(lastResult is TextCommandResult lastTextCommandResult) return lastTextCommandResult;
            return TextCommandResult.Success(lastResult is null ? string.Empty : lastResult.ToString(), lastResult);
        }

        return new TextCommandResult
        {
            Status = EnumCommandStatus.Success,
            StatusMessage = "insanitylib:cmd-result-combination",
            MessageParams = [
                combinationCount,
                failedCount,
                skippedCollectionItems.Sum()
            ]
        };
        
    }
}