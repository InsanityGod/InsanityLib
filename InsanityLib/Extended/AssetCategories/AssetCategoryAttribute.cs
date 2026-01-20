using InsanityLib.Auto.Cleanup;
using InsanityLib.Generators.Attributes;
using InsanityLib.Util;
using System;
using System.Linq;
using Vintagestory.API.Common;

namespace InsanityLib.Extended.AssetCategories;

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

    internal ExtendedAssetCategory CreateForType(Type type)
    {
        if (AssetCategory.categories.TryGetValue(Code, out var existing))
        {
            var existingType = existing is ExtendedAssetCategory extended ? extended.ClassType : existing.GetType();
            throw new InvalidOperationException($"[InsanityLib] [{type.FindModName()}] [{existingType.FindModName()}] Duplicate AssetCategory '{Code}' for both '{type.FullName}' and '{existingType.FullName}'");
        }
        return new(type, Code, AffectsGameplay, SideType);
    }

    public static bool Loaded { get; private set; }
    internal static void Load()
    {
        if(Loaded) return;
        foreach((var type, var attr) in ReflectionUtil.FindAllClasses<AssetCategoryAttribute>())
        {
            attr.CreateForType(type);
        }
        Loaded = true;
    }

    [DisposalLogic]
    internal static void Unload()
    {
        foreach(var category in AssetCategory.categories.Values.OfType<ExtendedAssetCategory>().ToList())
        {
            AssetCategory.categories.Remove(category.Code);
        }
        Loaded = false;
    }
}
