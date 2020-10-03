using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AccessTokenClient.Tests.Helpers
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode httpStatusCode;

        private readonly string response;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockHttpMessageHandler" /> class.
        /// </summary>
        /// <param name="response">The response mock response to use.</param>
        /// <param name="httpStatusCode">The HTTP status code to return.</param>
        public MockHttpMessageHandler(string response, HttpStatusCode httpStatusCode)
        {
            this.response       = response;
            this.httpStatusCode = httpStatusCode;
        }

        public string Input { get; private set; }

        public int NumberOfCalls { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            NumberOfCalls++;

            if (request.Content != null)
            {
                Input = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return new HttpResponseMessage
            {
                StatusCode = httpStatusCode,
                Content    = new StringContent(response)
            };
        }
    }
}