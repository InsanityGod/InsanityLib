using System;

namespace InsanityLib.Interfaces.Reflection;

public interface ITypeAssociated
{
    /// <summary>
    /// The associated type (generally represens the type of the object contained by the wrapper)
    /// </summary>
    Type AssociatedType { get; }
}
