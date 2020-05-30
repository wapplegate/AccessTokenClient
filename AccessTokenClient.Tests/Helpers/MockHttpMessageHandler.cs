using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AccessTokenClient.Tests.Helpers
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string response;

        private readonly HttpStatusCode httpStatusCode;

        public string Input { get; private set; }

        public int NumberOfCalls { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MockHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="response">The response mock response to use.</param>
        /// <param name="httpStatusCode">The HTTP status code to return.</param>
        public MockHttpMessageHandler(string response, HttpStatusCode httpStatusCode)
        {
            this.response = response;
            this.httpStatusCode = httpStatusCode;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            NumberOfCalls++;
            Input = await request.Content.ReadAsStringAsync();

            return new HttpResponseMessage
            {
                StatusCode = httpStatusCode,
                Content = new StringContent(response)
            };
        }
    }
}