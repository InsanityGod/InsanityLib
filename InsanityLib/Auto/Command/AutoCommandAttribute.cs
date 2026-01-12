using InsanityLib.Constants;
using InsanityLib.Exceptions;
using InsanityLib.Util;
using System;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace InsanityLib.Auto.Command;

[AttributeUsage(AttributeTargets.Method)]
public class AutoCommandAttribute : Attribute
{
    /// <summary>
    /// What side to create the command on (if both is selected it will create one for each side)<br/>
    /// If not set it will be determined based on the wether the method accepts <see cref="ICoreClientAPI"/> or <see cref="ICoreServerAPI"/> parameters.
    /// </summary>
    public EnumAppSide Side { get; set; } = 0;

    /// <summary>
    /// Name of the command
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The path this command should be registered under
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// The privelege required to run this
    /// </summary>
    public string? RequiredPrivelege { get; set; }

    /// <summary>
    /// Wether a player is required
    /// </summary>
    public bool RequiresPlayer { get; set; }

    public void RegisterIfNeeded(ICoreAPI api, MethodBase method)
    {
        if(Side == 0)
        {
            var parameters = method.GetParameters();

            var mustBeClient = Array.Exists(parameters, info => info.ParameterType == typeof(ICoreClientAPI));
            var mustBeServer = Array.Exists(parameters, info => info.ParameterType == typeof(ICoreServerAPI));

            if(mustBeClient && mustBeServer) throw new InvalidOperationException($"AutoCommand accepts both {nameof(ICoreClientAPI)} and {nameof(ICoreServerAPI)} at the same time, cannot automatically determine side.");
            
            if(mustBeClient) Side = EnumAppSide.Client;
            else if(mustBeServer) Side = EnumAppSide.Server;
            else Side = EnumAppSide.Server;
        }

        if((api.Side & Side) == 0) return;

        var name = string.IsNullOrWhiteSpace(Name) ? method.Name.ToLower() : Name;
        var path = string.IsNullOrWhiteSpace(Path) ? name : $"{Path}/{name}";
        try
        {
            var autoCommand = new AutoCommand(method, path, RequiresPlayer)
            { 
                RequiredPermission = RequiredPrivelege
            };

            autoCommand.GetOrRegister(api);
        }
        catch(InvalidAttributeUsageException ex)
        {
            api.Logger.Error(Logging.AutoCommandSetupFailed, method.DeclaringType?.Assembly.FindMod(api)?.Info.Name, method.GetDebugDisplayName(), ex.Message);
        }
        catch(Exception ex)
        {
            api.Logger.Error(Logging.AutoCommandSetupFailed, method.DeclaringType?.Assembly.FindMod(api)?.Info.Name, method.GetDebugDisplayName(), ex);
        }
    }

    internal static void FindAndRegisterAutoCommands(ICoreAPI api)
    {
        foreach((var member, var attr) in ReflectionUtil.FindAllMembersWithAttributes<AutoCommandAttribute>())
        {
            try
            {
                if(member is not MethodBase method) throw new InvalidOperationException($"member '{member}' was not a method despite being marked as AutoCommandAttribute");
                attr.RegisterIfNeeded(api, method);
            }
            catch(Exception ex)
            {
                api.Logger.Error(Logging.ExecutionFailed, nameof(FindAndRegisterAutoCommands), member, ex);
            }
        }
    }
}
