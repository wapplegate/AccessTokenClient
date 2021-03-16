using AccessTokenClient.Tests.Helpers;
using Xunit;

namespace AccessTokenClient.Tests
{
    public class HttpRequestMessageBuilderTests
    {
        [Fact]
        public void EnsureRequestMessageGeneratedSuccessfully()
        {
            var builder = new HttpRequestMessageBuilder();
            var message = builder.GenerateHttpRequestMessage(new TokenRequest
            {
                TokenEndpoint    = "https://www.test.com/token",
                ClientIdentifier = "testing_client_identifier",
                ClientSecret     = "testing_client_secret",
                Scopes           = new []{ "scope:read" }
            });
            message.ShouldNotBeNull();
        }
    }
}