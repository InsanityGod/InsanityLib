using InsanityLib.Auto.Config;
using InsanityLib.Config.Sub;
using InsanityLib.Generators.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Common;

namespace InsanityLib.Config;

public class InsanityLibConfig
{
    [AutoConfig("InsanityLibConfig.json", ServerSync = false)] 
    public static InsanityLibConfig? Instance { get; set; }
    
    public AutoConfigConfig AutoConfig { get; set; } = new AutoConfigConfig();

    public EnumAppSide Side { get; set; }

    public EnumAppSide Side2 { get; set; } = EnumAppSide.Universal;

    public EnumBlockAccessFlags BlockAccessFlags { get; set; }
    public EnumBlockAccessFlags BlockAccessFlags2 { get; set; } = EnumBlockAccessFlags.Use | EnumBlockAccessFlags.Traverse;

    public EnumBlockMaterial BlockMaterial { get; set; }

    /// <summary>
    /// Just a value for testing purposed
    /// </summary>
    [Range(5, 10)]
    public int Test { get; set; }

    /// <summary>
    /// Just a value for testing purposed
    /// </summary>
    [Range(0, 1)]
    public double? TestPercentage { get; set; }

    /// <summary>
    /// Just some random string input
    /// </summary>
    public string SomeInput { get; set; } = string.Empty;

    public int? NullableNumber { get; set; }

    public int Number { get; set; }

    [Required]
    public int? NullableButRequired { get; set;}

    /// <summary>
    /// Just some random nullable string input
    /// </summary>
    [AllowNull]
    public string SomeNullableInput { get; set; }

    /// <summary>
    /// Just some random nullable string input
    /// </summary>
    public string? AnotherNullableInput { get; set; }

    /// <summary>
    /// Just some random dropdown
    /// </summary>
    [AllowedValues("Foo", "Bar")] //TODO fix in config display
    public string SelectAValue { get; set; } = string.Empty;

    /// <summary>
    /// An extra file to put custom configuration in
    /// </summary>
    public AssetLocation ExtraDocumentationFile { get; set;} = new AssetLocation("insanitylib", "config_extra");
    
    /// <summary>
    /// again but not nullable
    /// </summary>
    public AssetLocation? ExtraDocumentationFileNullable { get; set;}

}
