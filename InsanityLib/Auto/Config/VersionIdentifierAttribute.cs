using InsanityLib.Constants;
using InsanityLib.Extensions;
using InsanityLib.Util;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Vintagestory.API.Common;

namespace InsanityLib.Auto.Config;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class VersionIdentifierAttribute(object expectedValue) : Attribute
{
    public object ExpectedValue { get; } = expectedValue;

    public EConfigVersionUpgradeMode UpgradeMode { get; init; } = EConfigVersionUpgradeMode.MergeIntoNew;

    /// <summary>
    /// Validates the current member value against the expected value.<br/>
    /// If the value is not valid, it will attempt to fix it based on the upgrade mode.
    /// </summary>
    /// <exception cref="InvalidOperationException">When attribute is applied to member of which value can not be auto aquired</exception>
    /// <exception cref="ValidationException">When the check failed and UpgradeMode is set to EConfigVersionUpgradeMode.Throw</exception>
    /// <exception cref="NotImplementedException">When UpgradeMode contains an invalid/unknown value</exception>
    /// <returns>Wether it tried fixing based on UpgradeMode (only if UpgradeMode is valid and not EConfigVersionUpgradeMode.Throw)</returns>
    public virtual bool ValidateAndFix(IServiceProvider provider, MemberInfo member, ref object instance, string path)
    {
        if(!member.CanGetAutoValue(provider)) throw new InvalidOperationException($"{member} is not a valid target for [{nameof(VersionIdentifierAttribute)}] since it value cannot be automatically procured");
        var value = member.GetAutoValue(provider, instance);
        if (ExpectedValue.Equals(value)) return false;

        switch(UpgradeMode)
        {
            case EConfigVersionUpgradeMode.MergeIntoNew:
                provider.GetService<ILogger>()?.Warning(Logging.AutoConfigUpdate, path, member, ExpectedValue, value);

                var newInstance = (JContainer) JToken.FromObject(instance.GetType().AutoCreate(provider, false));

                newInstance.Merge(JToken.FromObject(instance), new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Merge,
                });
                instance = newInstance.ToObject(instance.GetType())!;
                if (member.CanSetValue()) member.SetValue(ExpectedValue.AutoConvert(member.GetPrimaryType()), instance);

                return true;

            case EConfigVersionUpgradeMode.Warning:
                provider.GetService<ILogger>()?.Warning(Logging.ConfigOutdated, member, path, ExpectedValue, value);
                return false;
            case EConfigVersionUpgradeMode.Throw: throw new ValidationException(string.Format(Logging.ConfigOutdated, member, path, ExpectedValue, value));
            default: throw new NotImplementedException($"Version upgrade mode {UpgradeMode} is not implemented");
        }
    }
}
