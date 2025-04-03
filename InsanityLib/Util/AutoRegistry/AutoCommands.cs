using InsanityLib.Attributes.Auto.Command;
using InsanityLib.Constants;
using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace InsanityLib.Util.AutoRegistry
{
    public static class AutoCommands
    {
        public static void RegisterAutoCommands(this ICoreAPI api)
        {
            foreach((var member, var attr) in ReflectionUtil.FindAllMembers<AutoCommandAttribute>())
            {
                try
                {
                    if(member is not MethodBase method) throw new InvalidOperationException($"member '{member}' was not a method despite being marked as AutoCommandAttribute");
                    attr.SetDefaultValues(method, api);
                    if((attr.Side & api.Side) == 0) continue;
                    var parentCommand = GetParentCommand(api, attr.MainCommand);
                    
                    var command = parentCommand != null ? parentCommand.GetOrCreateChildStub(attr.Name) : GetOrCreateStub(api, attr.Name);
                    attr.ConfigureCommand(command, api.GetServiceContainer(), method);
                }
                catch(Exception ex)
                {
                    api.Logger.Error(Logging.ExecutionFailedTemplate, nameof(RegisterAutoCommands), member, ex);
                }
            }
        }

        public static IChatCommand GetParentCommand(ICoreAPI api, string path)
        {
            path = path?.Trim('/', ' ');
            if(string.IsNullOrEmpty(path)) return null;

            var steps = path.Split('/');

            var command = GetOrCreateStub(api, steps[0]);

            for(var i = 1; i < steps.Length; i++) command = command.GetOrCreateChildStub(steps[i]);

            return command;
        }

        public static IChatCommand GetOrCreateStub(ICoreAPI api, string name) => api.ChatCommands.GetOrCreate(name)
            .WithDefaultConfiguration();
        public static IChatCommand GetOrCreateChildStub(this IChatCommand command, string name) => command.BeginSubCommand(name)
            .WithDefaultConfiguration();

        public static IChatCommand WithDefaultConfiguration(this IChatCommand command)
        {
            if(command.Incomplete) command.RequiresPrivilege(Privilege.chat);
            return command;
        }

        public static TextCommandResult NoSuchCommand(TextCommandCallingArgs callingArgs) => new() { Status = EnumCommandStatus.NoSuchCommand };
    }
}
