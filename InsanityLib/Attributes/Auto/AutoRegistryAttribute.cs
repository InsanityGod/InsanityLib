using HarmonyLib;
using InsanityLib.Constants;
using InsanityLib.Handlers;
using InsanityLib.Interfaces;
using InsanityLib.Util;
using InsanityLib.Util.ContentFeatures;
using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace InsanityLib.Attributes.Auto
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AutoRegistryAttribute : Attribute
    {
        public readonly string Domain;

        public AutoRegistryAttribute(string domain) => Domain = domain;

        internal static void RegisterAll(ICoreAPI api)
        {
            var logger = api.GetService<ILogger>();

            AutoRegistryAttribute attr = null;
            foreach (var assembly in AccessTools.AllAssemblies())
            {
                try
                {
                    assembly.GetCustomAttribute<AutoRegistryAttribute>()?.RegisterAssembly(assembly, api);
                }
                catch (Exception ex)
                {
                    logger?.Error(Logging.ExecutionFailedTemplate, nameof(AutoRegistryAttribute), attr is not null ? attr.Domain : assembly, ex);
                }
            }
        }

        private void RegisterAssembly(Assembly assembly, ICoreAPI api)
        {
            //Base game
            foreach(var itemClass in assembly.FindImplementations<Item>()) api.RegisterItemClass(itemClass.GetRegistryName(Domain), itemClass);
            foreach(var blockClass in assembly.FindImplementations<Block>()) api.RegisterBlockClass(blockClass.GetRegistryName(Domain), blockClass);
            foreach(var blockEntityClass in assembly.FindImplementations<BlockEntity>()) api.RegisterBlockEntityClass(blockEntityClass.GetRegistryName(Domain), blockEntityClass);

            foreach(var collectibleBehaviorClass in assembly.FindImplementations<CollectibleBehavior>())
            {
                if (typeof(BlockBehavior).IsAssignableFrom(collectibleBehaviorClass))
                {
                    api.RegisterBlockBehaviorClass(collectibleBehaviorClass.GetRegistryName(Domain), collectibleBehaviorClass);
                }
                else
                {
                    api.RegisterCollectibleBehaviorClass(collectibleBehaviorClass.GetRegistryName(Domain), collectibleBehaviorClass);
                }
            }

            foreach (var blockEntityBehaviorClass in assembly.FindImplementations<BlockEntityBehavior>()) api.RegisterBlockEntityBehaviorClass(blockEntityBehaviorClass.GetRegistryName(Domain), blockEntityBehaviorClass);

            //Content Featurs
            foreach (var transitionHandlerClass in assembly.FindImplementations<TransitionHandler>(includeSelf:true)) CustomTransition.RegisterHandler(transitionHandlerClass.GetRegistryName(Domain), transitionHandlerClass);
        }
    }
}
