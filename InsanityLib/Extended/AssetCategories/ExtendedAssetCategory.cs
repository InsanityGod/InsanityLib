using InsanityLib.Interfaces.Reflection;
using System;
using Vintagestory.API.Common;

namespace InsanityLib.Extended.AssetCategories;

public class ExtendedAssetCategory(Type type, string code, bool AffectsGameplay, EnumAppSide SideType) : AssetCategory(code, AffectsGameplay, SideType), ITypeAssociated
{
    public Type AssociatedType { get; } = type;
}
