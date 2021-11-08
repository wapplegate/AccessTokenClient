using FluentAssertions;
using JetBrains.Annotations;

namespace AccessTokenClient.Tests.Helpers;

public static class FluentAssertionsExtensions
{
    /// <summary>
    /// Asserts that the object to test is not null. Includes a
    /// contract annotation that allows Resharper code analysis
    /// to recognize accessing the object after this call is made
    /// will not result in a null reference exception.
    /// </summary>
    /// <param name="objectToTest">The object to test.</param>
    [ContractAnnotation("null => stop")]
    public static void ShouldNotBeNull(this object objectToTest)
    {
        objectToTest.Should().NotBeNull();
    }
}