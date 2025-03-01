using AccessTokenClient.Caching;
using Shouldly;
using Xunit;

namespace AccessTokenClient.Tests;

public class DefaultAccessTokenTransformerTests
{
    [Fact]
    public void EnsureConvertedValueIsTheSameAsOriginal()
    {
        const string Value = "test";

        var transformer = new DefaultAccessTokenTransformer();
        var convertedValue = transformer.Convert(Value);

        convertedValue.ShouldNotBeNull();
        convertedValue.ShouldBe(Value);
    }

    [Fact]
    public void EnsureRevertedValueIsTheSameAsOriginal()
    {
        const string Value = "test";

        var transformer = new DefaultAccessTokenTransformer();
        var revertedValue = transformer.Revert(Value);

        revertedValue.ShouldNotBeNull();
        revertedValue.ShouldBe(Value);
    }
}
