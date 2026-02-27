using InsanityLib.Util.Span;
using Vintagestory.API.Common;

namespace InsanityLib.Tests;

public class AlternativeLookupTests
{
    private readonly Dictionary<AssetLocation, string> test = new(AssetLocationSpanComparer.Instance)
    {
        [new AssetLocation("test1", "someasset")] = "Some Asset",
        [new AssetLocation("test2","anotherasset")] = "Another Asset",
        [new AssetLocation("test3", "helloworld")] = "Hello World"
    };

    [Fact]
    public void AssetLocationSpanLookup()
    {
        var toFind = "test2:anotherasset";

        var lookup = test.GetAlternateLookup<AssetLocationSpan>();


        Assert.True(lookup.TryGetValue(toFind, out var result));
        Assert.Equal("Another Asset", result);
    }
}