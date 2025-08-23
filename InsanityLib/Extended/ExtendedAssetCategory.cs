using System;
using Vintagestory.API.Common;

namespace InsanityLib.Extended;

public class ExtendedAssetCategory : AssetCategory
{
    public ExtendedAssetCategory(Type type, string code, bool AffectsGameplay, EnumAppSide SideType) : base(code, AffectsGameplay, SideType) => ClassType = type;

    public Type ClassType { get; }
}
