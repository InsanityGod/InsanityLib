using System;
using System.ComponentModel.DataAnnotations;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace InsanityLib.Auto.Command.Argument.Providers;

public static class ProviderValidationExtensions
{
    public static Entity Required(this Entity entity, EContextualSource source, Type type) => entity ?? throw new ValidationException($"Contextual source '{source}' for type '{type}' requires the command to be called by an Entity");
    
    public static IPlayer Required(this IPlayer player, EContextualSource source, Type type) => player ?? throw new ValidationException($"Contextual source '{source}' for type '{type}' requires the command to be called by a Player");

    public static Vec3d PosRequired(this Vec3d pos) => pos ?? throw new ValidationException($"The position could not be determined from the command caller");
}
