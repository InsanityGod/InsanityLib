namespace InsanityLib.Extensions;

public static class NumberExtensions
{
    public static string ToPercentageString(this float percentage) => string.Format("{0:P0}", percentage);
}
