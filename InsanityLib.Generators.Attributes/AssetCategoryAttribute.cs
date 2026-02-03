using System.Diagnostics;
using Vintagestory.API.Common;

namespace InsanityLib.Generators.Attributes;

[Conditional("CompileTimeOnly")]
[AttributeUsage(AttributeTargets.Class)]
public class AssetCategoryAttribute(string code, bool affectsGameplay, EnumAppSide sideType) : Attribute
{

    /// <summary>
    /// Path and name
    /// </summary>
    public string Code { get; } = code;

    /// <summary>
    /// Determines wether it will be used on server, client or both.
    /// </summary>
    public EnumAppSide SideType { get; } = sideType;

    /// <summary>
    /// Temporary solution to not change block types. Will be changed
    /// </summary>
    public bool AffectsGameplay { get; } = affectsGameplay;

    //internal ExtendedAssetCategory CreateForType(Type type)
    //{
    //    if (AssetCategory.categories.TryGetValue(Code, out var existing))
    //    {
    //        var existingType = existing is ExtendedAssetCategory extended ? extended.ClassType : existing.GetType();
    //        throw new InvalidOperationException($"[InsanityLib] [{type.FindModName()}] [{existingType.FindModName()}] Duplicate AssetCategory '{Code}' for both '{type.FullName}' and '{existingType.FullName}'");
    //    }
    //    return new(type, Code, AffectsGameplay, SideType);
    //}
}
