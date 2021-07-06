using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AccessTokenClient.Caching
{
    public class DistributedTokenResponseCache : ITokenResponseCache
    {
        private readonly IDistributedCache cache;

        public DistributedTokenResponseCache(IDistributedCache cache)
        {
            this.cache = cache;
        }

        public Task<TokenResponse> Get(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Set(string key, TokenResponse response, TimeSpan expiration, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}