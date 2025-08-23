using InsanityLib.Attributes.Auto;
using InsanityLib.Extended;
using InsanityLib.Util;
using System;
using System.Linq;
using Vintagestory.API.Common;

namespace InsanityLib.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class AssetCategoryAttribute : Attribute
{
    public AssetCategoryAttribute(string code, bool affectsGameplay,EnumAppSide sideType)
    {
        Code = code;
        AffectsGameplay = affectsGameplay;
        SideType = sideType;
    }

    /// <summary>
    /// Path and name
    /// </summary>
    public string Code { get; }

		/// <summary>
		/// Determines wether it will be used on server, client or both.
		/// </summary>
		public EnumAppSide SideType { get; }

		/// <summary>
		/// Temporary solution to not change block types. Will be changed
		/// </summary>
		public bool AffectsGameplay { get; }

    internal ExtendedAssetCategory CreateForType(Type type)
    {
        if (AssetCategory.categories.TryGetValue(Code, out var existing))
        {
            var existingType = existing is ExtendedAssetCategory extended ? extended.ClassType.FullName : "Unknown";
            throw new InvalidOperationException($"[InsanityLib] Duplicate AssetCategory '{Code}' for both '{type.FullName}' and '{existingType}'");
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
