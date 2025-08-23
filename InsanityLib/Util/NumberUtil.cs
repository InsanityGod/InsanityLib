namespace InsanityLib.Util;

public static class NumberUtil
{
    public static string ToPercentageString(this float percentage) => string.Format("{0:P0}", percentage);
}
