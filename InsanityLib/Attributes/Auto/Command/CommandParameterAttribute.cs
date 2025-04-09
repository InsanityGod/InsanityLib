using HarmonyLib;
using InsanityLib.Commands;
using InsanityLib.Constants;
using InsanityLib.Enums.Auto.Commands;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace InsanityLib.Attributes.Auto.Command
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class CommandParameterAttribute : Attribute
    {
        public EParamSource Source { get; set; }

        public ICommandArgumentParser FindParser(ParameterInfo param, IServiceProvider provider)
        {
            Type paramType = param.ParameterType;
            
            var displayNameAttr = param.GetCustomAttribute<DisplayNameAttribute>();
            var argName = displayNameAttr != null ? displayNameAttr.DisplayName : param.Name;

            //TODO non mandetory parameters?

            if(paramType == typeof(bool)) return new BoolArgParser(argName, "on", true);
            else if(paramType == typeof(int))
            {
                var rangeAttr = param.GetCustomAttribute<RangeAttribute>();
                return new IntArgParser(argName, Convert.ToInt32(rangeAttr.Minimum ?? int.MinValue), Convert.ToInt32(rangeAttr.Maximum ?? int.MaxValue), param.HasDefaultValue ? (int)param.DefaultValue : 0, true);
            }
            else if(paramType == typeof(long))
            {
                var rangeAttr = param.GetCustomAttribute<RangeAttribute>();
                return new LongArgParser(argName, Convert.ToInt64(rangeAttr.Minimum ?? long.MinValue), Convert.ToInt64(rangeAttr.Maximum ?? long.MaxValue), param.HasDefaultValue ? (long)param.DefaultValue : 0, true);
            }
            else if(paramType == typeof(float))
            {
                var rangeAttr = param.GetCustomAttribute<RangeAttribute>();
                return new FloatArgParser(argName, Convert.ToSingle(rangeAttr.Minimum ?? float.MinValue), Convert.ToSingle(rangeAttr.Maximum ?? float.MaxValue), true);
            }
            else if(paramType == typeof(double))
            {
                var rangeAttr = param.GetCustomAttribute<RangeAttribute>();
                return new DoubleArgParser(argName, Convert.ToDouble(rangeAttr.Minimum ?? double.MinValue), Convert.ToDouble(rangeAttr.Maximum ?? double.MaxValue), true);
            }
            else if(paramType == typeof(string)) return new WordArgParser(argName, true); //TODO maybe something for longer text
            else if(typeof(Block).IsAssignableFrom(paramType))
            {
                if(Source == EParamSource.Specify) return new CollectibleArgParser(argName, provider.GetService<ICoreAPI>(), EnumItemClass.Block, true); 
            }
            else if(typeof(Item).IsAssignableFrom(paramType))
            {
                if(Source == EParamSource.Specify) return new CollectibleArgParser(argName, provider.GetService<ICoreAPI>(), EnumItemClass.Item, true);
            }
            else if(paramType == typeof(Color)) return new ColorArgParser(argName, true);
            else if(paramType == typeof(DateTime)) return new DatetimeArgParser(argName, true);
            //TODO parser (cannot cast this one)
            else if(paramType == typeof(BlockPos))
            {
                 if(Source == EParamSource.Specify) return new WorldPositionArgParser(argName, provider.GetService<ICoreAPI>(), true);
            }
            else if(paramType.IsEnum) return new WordRangeArgParser(argName, true, Enum.GetNames(paramType));

            return null;
        }

        public object GetValueFromParser(AutoCommand command, ParameterInfo paramInfo, int parserIndex)
        {
            //TODO look into default values
            var value = command.Context.Parsers[parserIndex].GetValue();
            
            //TODO more extensible converter
            if (paramInfo.ParameterType.IsEnum) return Enum.Parse(paramInfo.ParameterType, (string)value);
            else if (typeof(CollectibleObject).IsAssignableFrom(paramInfo.ParameterType))
            {
                var stack = (ItemStack)value;
                var collectible = stack?.Collectible;
                if(collectible == null || paramInfo.ParameterType.IsInstanceOfType(collectible)) return collectible;
                else throw new ValidationException($"Expected '{paramInfo.ParameterType}' as target collectible but got '{collectible.GetType()}'");
            }
            else return paramInfo.ParameterType.IsInstanceOfType(value) ? value : Convert.ChangeType(value, paramInfo.ParameterType);
        }
    }
}
