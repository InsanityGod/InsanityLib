using System;
using Vintagestory.API.Common;

namespace InsanityLib.Extended.AssetCategories;

public class ExtendedAssetCategory(Type type, string code, bool AffectsGameplay, EnumAppSide SideType) : AssetCategory(code, AffectsGameplay, SideType)
{
    public Type ClassType { get; } = type;
}
