using InsanityLib.Attributes.Auto.Command;
using InsanityLib.Constants;
using InsanityLib.Enums.Auto.Commands;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace InsanityLib.Commands
{
    public class AutoCommand
    {
        public readonly IServiceProvider provider;

        public readonly MethodBase command;

        public readonly ImmutableArray<EParamProvider> parameterProviders;

        public TextCommandCallingArgs Context { get; private set; }

        public AutoCommand(IServiceProvider provider, MethodBase method, IEnumerable<EParamProvider> EArgProviders)
        {
            this.provider = provider;
            command = method;
            parameterProviders = EArgProviders.ToImmutableArray();
        }

        public TextCommandResult RunCommand(TextCommandCallingArgs args)
        {
            try
            {
                Context = args;
                var result = command.Invoke(null, GetParameters());
                if (result is TextCommandResult textCommandResult) return textCommandResult;
                return TextCommandResult.Success(result == null ? string.Empty : result.ToString(), result);
            }
            catch (ValidationException ex)
            {
                return TextCommandResult.Error(ex.Message);
            }
            catch (Exception ex)
            {
                provider.GetService<ILogger>()?.Error(Logging.ExecutionFailedTemplate, nameof(RunCommand), command, ex);
                return TextCommandResult.Error(ex.InnerException?.Message);
            }
            finally
            {
                Context = null;
            }
        }

        private object[] GetParameters()
        {
            var parameterInfos = command.GetParameters();
            var parameters = new object[parameterInfos.Length];
            var paramIndex = 0;
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                var param = parameterInfos[i];
                var attr = param.GetCustomAttribute<CommandParameterAttribute>();

                switch (parameterProviders[i])
                {
                    case EParamProvider.ServiceProvider:
                        parameters[i] = provider.GetService(param.ParameterType); //TODO allow for overiding with attribute
                        break;

                    case EParamProvider.ArgumentParser:
                        parameters[i] = attr.GetValueFromParser(this, param, paramIndex++);
                        break;

                    case EParamProvider.Custom:
                        //TODO make more extensible framework for this

                        if (param.ParameterType == typeof(IPlayer)) parameters[i] = attr.Source switch
                        {
                            EParamSource.Caller => Context.Caller.Player,
                            EParamSource.CallerTarget => Context.Caller.Entity?.GetTargetEntity(),
                            _ => throw new InvalidOperationException($"Cannot inject {param.Name}, invalid parameter source '{attr.Source}' for custom provider of type '{param.ParameterType}'"),
                        };
                        else if (param.ParameterType == typeof(Entity)) parameters[i] = attr.Source switch
                        {
                            EParamSource.Caller => Context.Caller.Entity,
                            EParamSource.CallerTarget => Context.Caller.Entity?.GetTargetEntity(),
                            _ => throw new InvalidOperationException($"Cannot inject {param.Name}, invalid parameter source '{attr.Source}' for custom provider of type '{param.ParameterType}'"),
                        };
                        else if (param.ParameterType == typeof(ItemSlot)) parameters[i] = attr.Source switch
                        {
                            EParamSource.Caller => (Context.Caller.Entity as EntityAgent)?.ActiveHandItemSlot,
                            _ => throw new InvalidOperationException($"Cannot inject {param.Name}, invalid parameter source '{attr.Source}' for custom provider of type '{param.ParameterType}'"),
                        };
                        else if (param.ParameterType == typeof(ItemStack)) parameters[i] = attr.Source switch
                        {
                            EParamSource.Caller => (Context.Caller.Entity as EntityAgent)?.ActiveHandItemSlot.Itemstack,
                            _ => throw new InvalidOperationException($"Cannot inject {param.Name}, invalid parameter source '{attr.Source}' for custom provider of type '{param.ParameterType}'"),
                        };
                        else if (param.ParameterType == typeof(Item)) parameters[i] = attr.Source switch
                        {
                            EParamSource.Caller => (Context.Caller.Entity as EntityAgent)?.ActiveHandItemSlot.Itemstack?.Item,
                            _ => throw new InvalidOperationException($"Cannot inject {param.Name}, invalid parameter source '{attr.Source}' for custom provider of type '{param.ParameterType}'"),
                        };
                        else if (typeof(Block).IsAssignableFrom(param.ParameterType))
                        {
                            var block = GetBlock(attr, param);
                            if (param.ParameterType.IsInstanceOfType(block)) parameters[i] = block;
                        }
                        else if (typeof(CollectibleObject).IsAssignableFrom(param.ParameterType))
                        {
                            var collectible = GetCollectible(attr, param);
                            if (param.ParameterType.IsInstanceOfType(collectible)) parameters[i] = collectible;
                        }
                        else if (param.ParameterType == typeof(BlockPos)) parameters[i] = attr.Source switch
                        {
                            EParamSource.Caller => Context.Caller.Pos?.AsBlockPos,
                            EParamSource.CallerTarget => Context.Caller.Player?.CurrentBlockSelection?.Position,
                            _ => throw new InvalidOperationException($"Cannot inject {param.Name}, invalid parameter source '{attr.Source}' for custom provider of type '{param.ParameterType}'"),
                        };
                        else if (typeof(BlockBehavior).IsAssignableFrom(param.ParameterType)) parameters[i] = GetBlock(attr, param)?.BlockBehaviors.SingleOrDefault(param.ParameterType.IsInstanceOfType);
                        else if (typeof(BlockEntity).IsAssignableFrom(param.ParameterType))
                        {
                            var blockEntity = GetBlockEntity(attr, param);
                            if (param.ParameterType.IsInstanceOfType(blockEntity)) parameters[i] = blockEntity;
                        }
                        else if (typeof(BlockEntityBehavior).IsAssignableFrom(param.ParameterType)) parameters[i] = GetBlockEntity(attr, param)?.Behaviors.SingleOrDefault(param.ParameterType.IsInstanceOfType);

                        break;
                }
            }

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                var param = parameterInfos[i];
                var validationAttributes = param.GetCustomAttributes<ValidationAttribute>();
                foreach (var validationAttribute in validationAttributes)
                {
                    var context = new ValidationContext(parameterInfos[i]);
                    var validationResult = validationAttribute.GetValidationResult(parameters[i], context);
                    if (validationResult != ValidationResult.Success) throw new ValidationException(validationResult, validationAttribute, parameters[i]);
                }
            }

            return parameters;
        }

        private Block GetBlock(CommandParameterAttribute attr, ParameterInfo param) => attr.Source switch
        {
            EParamSource.Caller => (Context.Caller.Entity as EntityAgent)?.ActiveHandItemSlot.Itemstack?.Block,
            EParamSource.CallerTarget => Context.Caller.Player?.CurrentBlockSelection?.GetOrFindBlock(provider.GetService<IWorldAccessor>()),
            _ => throw new InvalidOperationException($"Cannot inject {param.Name}, invalid parameter source '{attr.Source}' for custom provider of type '{param.ParameterType}'"),
        };

        private CollectibleObject GetCollectible(CommandParameterAttribute attr, ParameterInfo param) => attr.Source switch
        {
            EParamSource.Caller => (Context.Caller.Entity as EntityAgent)?.ActiveHandItemSlot.Itemstack?.Collectible,
            _ => throw new InvalidOperationException($"Cannot inject {param.Name}, invalid parameter source '{attr.Source}' for custom provider of type '{param.ParameterType}'"),
        };

        private BlockEntity GetBlockEntity(CommandParameterAttribute attr, ParameterInfo param) => attr.Source switch
        {
            EParamSource.CallerTarget => Context.Caller.Player?.CurrentBlockSelection?.FindBlockEntity(provider.GetService<IWorldAccessor>()),
            _ => throw new InvalidOperationException($"Cannot inject {param.Name}, invalid parameter source '{attr.Source}' for custom provider of type '{param.ParameterType}'"),
        };
    }
}