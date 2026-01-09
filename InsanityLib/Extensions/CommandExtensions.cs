using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace InsanityLib.Extensions;

public static class CommandExtensions
{
    /// <summary>
    /// Gets or creates a command with the given path and ensures it's not  <see cref="IChatCommand.Incomplete"/>. <br/>
    /// </summary>
    public static IChatCommand GetOrCreateStub(this IChatCommandApi chatCommandApi, string path, IChatCommand? parentCommand = null)
    {
        if(path.Contains('/'))
        {
            var pathParts = path.Split('/');
            foreach (var pathPart in pathParts) parentCommand = chatCommandApi.GetOrCreateStub(pathPart, parentCommand);

            return parentCommand!;
        }
        
        return parentCommand is not null
            ? parentCommand.BeginSubCommand(path).WithDefaultConfiguration()
            : chatCommandApi.GetOrCreate(path).WithDefaultConfiguration();
    }

    /// <summary>
    /// Ensure that the command is not <see cref="IChatCommand.Incomplete"/>
    /// </summary>
    public static IChatCommand WithDefaultConfiguration(this IChatCommand command) => command.Incomplete ? command.RequiresPrivilege(Privilege.chat) : command;

    public static TextCommandResult NoSuchCommand(TextCommandCallingArgs callingArgs) => new() { Status = EnumCommandStatus.NoSuchCommand };
}
