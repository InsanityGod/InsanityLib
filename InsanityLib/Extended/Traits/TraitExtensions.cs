using InsanityLib.Extended.Traits.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace InsanityLib.Extended.Traits;

public static class TraitExtensions
{
    public static bool AllowesSystem(this ITraitSystemConstraint constraint, ETraitSystem system) => (constraint.TraitSystems & system) != 0;

    public static IEnumerable<T> ForSystem<T>(this IEnumerable<T> collection, ETraitSystem system) where T : ITraitSystemConstraint => collection.Where(item => item.AllowesSystem(system));
}
