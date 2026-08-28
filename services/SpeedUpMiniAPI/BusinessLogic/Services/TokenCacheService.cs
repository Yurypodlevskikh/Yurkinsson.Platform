using BusinessLogic.Interfaces;
using BusinessLogic.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services
{
    internal class TokenCacheService : ITokenCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration;

        public TokenCacheService(IMemoryCache cache, IOptions<IdentityApiSettings> settings)
        {
            _cache = cache;
            _cacheDuration = settings.Value.CacheDuration;
        }
        public bool TryGetTokenInfo(string token, out CachedTokenInfo? cachedTokenInfo)
        {
            return _cache.TryGetValue(token, out cachedTokenInfo);
        }

        public void SetTokenInfo(string token, CachedTokenInfo cachedTokenInfo)
        {
            if (cachedTokenInfo == null)
                throw new ArgumentNullException(nameof(cachedTokenInfo));

            _cache.Set(token, cachedTokenInfo, cachedTokenInfo.TokenExpiry.Subtract(DateTime.UtcNow));
        }

        public void RemoveTokenInfo(string token)
        {
            _cache.Remove(token);
        }
    }
}