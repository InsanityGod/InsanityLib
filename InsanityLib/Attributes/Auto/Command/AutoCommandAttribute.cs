using InsanityLib.Commands;
using InsanityLib.Enums.Auto.Commands;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace InsanityLib.Attributes.Auto.Command
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AutoCommandAttribute : Attribute
    {

        /// <summary>
        /// What side to create the command on (if both is selected it will create variants)
        /// </summary>
        public EnumAppSide Side { get; set; } = EnumAppSide.Server;

        /// <summary>
        /// Name of the command
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The path this command should be registered under
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The privelege required to run this
        /// </summary>
        public string RequiredPrivelege { get; set; }

        /// <summary>
        /// Wether a player is required
        /// </summary>
        public bool RequiresPlayer { get; set; }

        public void SetDefaultValues(MethodBase method, ICoreAPI api)
        {
            var parameters = method.GetParameters();

            var mustBeClient = Array.Exists(parameters, info => info.ParameterType == typeof(ICoreClientAPI));
            var mustBeServer = Array.Exists(parameters, info => info.ParameterType == typeof(ICoreServerAPI));

            if(mustBeClient && mustBeServer) throw new InvalidOperationException($"AutoCommand cannot accept both {nameof(ICoreClientAPI)} and {nameof(ICoreServerAPI)} at the same time, accept {nameof(ICoreAPI)} instead");
            
            if(mustBeClient) Side = EnumAppSide.Client;
            else if(mustBeServer) Side = EnumAppSide.Server;

            Name ??= method.Name;
        }

        public void ConfigureCommand(IChatCommand command, IServiceProvider provider, MethodBase method)
        {
            var parameters = method.GetParameters();
            var args = new List<ICommandArgumentParser>();
            var argSources = new List<EParamProvider>();
            
            foreach(var param in parameters)
            {
                var attr = param.GetCustomAttribute<CommandParameterAttribute>();
                if(attr is null)
                {
                    argSources.Add(EParamProvider.ServiceProvider);
                    continue;
                }

                var parser = attr.FindParser(param, provider);
                if(parser is not null)
                {
                    args.Add(parser);
                    argSources.Add(EParamProvider.ArgumentParser);
                    continue;
                }

                argSources.Add(EParamProvider.Custom);
            }

            command.WithArgs(args.ToArray());

            if(!string.IsNullOrEmpty(RequiredPrivelege)) command.RequiresPrivilege(RequiredPrivelege);
            if (RequiresPlayer) command.RequiresPlayer();

            var context = new AutoCommand(provider, method, argSources);
            command.HandleWith(context.RunCommand);

            //Handle Description and the likes

            var doc = method.GetDocumentationContext();

            var descriptionStr = doc.GetDescription();
            if(!string.IsNullOrEmpty(descriptionStr)) command.WithDescription(descriptionStr);

            var exmaplesStrings = doc.GetExamples();
            if (exmaplesStrings.Length > 0) command.WithExamples(exmaplesStrings);

            var returnStr = doc.GetReturn();
            if(!string.IsNullOrEmpty(returnStr)) command.WithAdditionalInformation($"Returns: {returnStr}");
        }
    }
}
