namespace InsanityLib.Extended.Traits;

public class TraitAttribute
{
    public double Value { get; set; }

    /// <summary>
    /// The value but per level (for XSkills) leave blank to have it auto increment using Level * Value
    /// </summary>
    public double[]? ValuePerLevel { get; set; }
}
