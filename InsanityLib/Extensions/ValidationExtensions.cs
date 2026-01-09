using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace InsanityLib.Extensions;

public static class ValidationExtensions
{
    public static bool TryValidate(this ParameterInfo parameterInfo, IServiceProvider serviceProvider, object value, out ValidationException exception)
    {
        exception = null;
        var validationAttributes = parameterInfo.GetCustomAttributes<ValidationAttribute>(true);

        var context = new ValidationContext(parameterInfo.Member, serviceProvider, null)
        {
            MemberName = parameterInfo.Name
        };

        foreach (var validationAttribute in validationAttributes)
        {
            var result = validationAttribute.GetValidationResult(value, context);
            if (result != ValidationResult.Success)
            {
                exception = new ValidationException(result, validationAttribute, value);
                return false;
            }
        }

        return true;
    }

    public static void Validate(this ParameterInfo parameterInfo, IServiceProvider serviceProvider, object value)
    {
        if (!parameterInfo.TryValidate(serviceProvider, value, out var exception))
        {
            throw exception!;
        }
    }
}
